import { Component, OnInit } from '@angular/core';
import { User } from './_models/user';
import { AccountService } from './_services/account.service';
import { PresenceService } from './_services/presence.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],

})


export class AppComponent implements OnInit {
  title = '阿姨我不想努力了';

  // any代表任何型態，較不安全。
  users: any;

  // dependency injection, make http request to API
  constructor(private accountService: AccountService, private presence: PresenceService) { }

  // 實作OnInit給AppComponent用，呼叫API，
  ngOnInit() {
    this.setCurrentUser();
  }

  setCurrentUser() {
    // localStorage有可能是null，所以要先判斷
    var result = localStorage.getItem('user')
    if (result) {
      const user: User = JSON.parse(result);
      if (user) {
        this.accountService.setCurrentUser(user);
        this.presence.createHubConnection(user);
      }

    }
  }
}
