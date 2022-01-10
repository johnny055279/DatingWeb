import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { BehaviorSubject } from 'rxjs';
import { take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class PresenceService {
  hubUrl = environment.hubUrl;
  private hubConnection: HubConnection;
  //BehaviorSubject is a generic subject, every observer can get BehaviorSubject value when subscribing
  // bacause BehaviorSubject keep current value
  private onlineUsersSource = new BehaviorSubject<string[]>([]);
  public onlineUser$ = this.onlineUsersSource.asObservable();
  constructor(private toastr: ToastrService, private router: Router) { }

  createHubConnection(user: User) {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl + 'presence', { accessTokenFactory: () => user.token })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start().catch(error => console.error(error));
    // 這裡的文字必須要與API的一致
    this.hubConnection.on("UserIsOnline", username => {
      console.log(username + " has connected");
      this.onlineUser$.pipe(take(1)).subscribe(usernames => this.onlineUsersSource.next([...usernames, username]));
    });

    this.hubConnection.on("UserIsOffline", username => {
      console.log(username + " has disconnected");
      this.onlineUser$.pipe(take(1)).subscribe(usernames => this.onlineUsersSource.next([...usernames.filter(n => n !== username)]));
    });

    // 更新所有使者資料並存起來
    this.hubConnection.on("GetOnlineUsers", (usernames: string[]) => {
      console.log("GetOnlineUsers");
      this.onlineUsersSource.next(usernames);
    });

    this.hubConnection.on("NewMessageReceived", (username, nickName) => {
      console.log("NewMessageReceived");
      this.toastr.info(`${nickName} has send you a new message!`).onTap.pipe(take(1)).subscribe(() => {
        this.router.navigateByUrl('/member/' + username + '?tab=3');
      });
    })

  }

  stopHubConnection() {
    this.hubConnection.stop().catch(error => console.error(error));
  }
}
