import { HttpClient } from '@angular/common/http';
import { mergeAnalyzedFiles } from '@angular/compiler';
import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Group } from '../_models/group';
import { Message } from '../_models/message';
import { User } from '../_models/user';
import { BusyService } from './busy.service';
import { getPaginationHeaders, getPaginationResult } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class MessageService {

  baseUrl = environment.apiUrl;
  hubUrl = environment.hubUrl;
  private messageThreadSource = new BehaviorSubject<Message[]>([]);
  private hubConnection: HubConnection
  public messageThread$ = this.messageThreadSource.asObservable();
  constructor(private http: HttpClient, private busyService: BusyService) { }

  getMessages(pageNum, pageSize, container) {
    let params = getPaginationHeaders(pageNum, pageSize);

    params = params.append('Container', container);
    return getPaginationResult<Message[]>(this.baseUrl + 'message', params, this.http);
  }

  getMessageThread(username: string) {
    return this.http.get<Message[]>(this.baseUrl + 'message/thread/' + username);
  }

  // invoke return a promise, we can add async to guarantee that we return a promise form this method
  async sendMessage(username: string, content: string) {
    // use invoke to execute api method, SendMessage is api method, then give it param(CreateMessageDto)
    return this.hubConnection.invoke('SendMessage', { RecipientUsername: username, Content: content }).catch(error => console.error(error));
  }

  deleteMessage(id: number) {
    return this.http.delete(this.baseUrl + 'message/' + id);
  }

  createHubConnection(user: User, otherUserName: string) {
    this.busyService.busy();
    this.hubConnection = new HubConnectionBuilder().withUrl(this.hubUrl + "message?user=" + otherUserName, {
      accessTokenFactory: () => user.token
    }).withAutomaticReconnect().build();

    this.hubConnection.start().catch(error => console.log(error)).finally(() => { this.busyService.idle() });

    this.hubConnection.on('ReceiveMessageThread', message => {
      this.messageThreadSource.next(message);
    });

    this.hubConnection.on('NewMessage', message => {
      this.messageThread$.pipe(take(1)).subscribe(messages => {
        // use spread operator to create a new array for BehaviorSubject
        this.messageThreadSource.next([...messages, message]);
      })
    });

    // find if there are unread message and mark read
    this.hubConnection.on("UpdateGroup", (group: Group) => {
      if (group.connections.some(n => n.username === otherUserName)) {
        this.messageThread$.pipe(take(1)).subscribe(messages => {
          messages.forEach(element => {
            if (!element.dateRead) {
              element.dateRead = new Date(Date.now());
            }
          });
          // create a new array, it shouldn't interfer with angular change tracking
          this.messageThreadSource.next([...messages]);
        });

      }
    })
  }

  stopHubConnection() {
    this.messageThreadSource.next([]);
    // 不只是離開頁面時會stop, 為了防止重複stop而出錯, 這裡判斷是否還存在hubConnection
    if (this.hubConnection) {
      this.hubConnection.stop();
    }
  }
}
