import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor
} from '@angular/common/http';
import { Observable } from 'rxjs';
import { AccountService } from '../_services/account.service';
import { User } from '../_models/user';
import { take } from 'rxjs/operators';

@Injectable()
export class JwtInterceptor implements HttpInterceptor {

  constructor(private accountService: AccountService) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    let currentUser: User | undefined;

    // 為了解決subscribe後還要unsubscribe
    // take()這個運算是會讓Observable進入complete狀態，而可以進入complete狀態的Observable是不需要額外再做Unsubscribe的行為。
    // 預設的http.get()本來就會complete(非streaming模式)
    this.accountService.currentUser$.pipe(take(1)).subscribe(user=> currentUser = user);

    if(currentUser){
      // 把Header加上Authorization以供傳到後端驗證。
      request = request.clone({
        setHeaders:{Authorization: `Bearer ${currentUser.token}`}

      });
    }

    // 如此一來每次的Request都可以進行驗證。
    return next.handle(request);
  }
}
