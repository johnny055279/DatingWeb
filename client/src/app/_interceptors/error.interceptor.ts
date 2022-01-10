import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { ToastrService } from 'ngx-toastr';
import { NavigationExtras, Router } from '@angular/router';
import { catchError } from 'rxjs/operators';

// Interceptor可以攔截HTTP請求，並做額外的處理。

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {

  constructor(
    private router: Router,
    private toastr: ToastrService,
  ) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    return next.handle(request).pipe(
      catchError(error => {

        if(error){
          switch (error.status){
            case 400:
              //判斷是否有errors(在console裡面的資訊)
              if(error.error.errors){
                const modalStateErrors = [];
                for (const key in error.error.errors){
                  if(error.error.errors[key]){
                    // push into array
                    modalStateErrors.push(error.error.errors[key])
                  }
                }
                // flat()是es2019的功能，要在tsconfig.json。
                // 將會回傳一個由原先陣列的子陣列串接而成的新陣列。
                throw modalStateErrors.flat();
              }
              else if(typeof(error.error) === 'object'){
                this.toastr.error(error.statusText, error.status);
              }
              else{
                this.toastr.error(error.error, error.status);
              }
              break;
            case 401:
              this.toastr.error(error.statusText === "OK" ? "驗證失敗。" : error.statusText, error.status);
              break;
            case 404:
              this.router.navigateByUrl('/notFound');
              break;
            case 500:
              const navigationExtras: NavigationExtras = {state: {error: error.error}};
              this.router.navigateByUrl('/serverError', navigationExtras);
              break;
            default:
              this.toastr.error("超出預期的錯誤");
              console.log(error);
              break;
          }
        }
        return throwError(error);
      })
    );
  }
}
