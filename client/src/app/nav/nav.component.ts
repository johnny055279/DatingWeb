import { Component, OnInit, TemplateRef, ViewEncapsulation } from '@angular/core';
import { BsDropdownConfig } from 'ngx-bootstrap/dropdown';
import { AccountService } from '../_services/account.service';
import { BsModalService, BsModalRef } from 'ngx-bootstrap/modal';
import { AlertComponent } from 'ngx-bootstrap/alert';
import { Router } from '@angular/router';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css'],
  providers: [
    {
      provide: BsDropdownConfig,
      useValue: {
        isAnimated: true,
        autoClose: true
      }
    }
  ],
  // 將CSS封裝解除，才可以自訂CSS覆蓋。
  encapsulation: ViewEncapsulation.None
})



export class NavComponent implements OnInit {

  // 定義出儲存使用者輸入的訊息，這裡用{}打包
  model: any = {};
  searchObj: any = {};
  modalRef!: BsModalRef;
  alerts: any[] = [{}];
  isCollapsed: boolean = true;

  constructor(
    // 如果要在網頁呼叫，就必須變成public
    public accountService: AccountService,
    private modalService: BsModalService,
    private router: Router,
  ) { }

  ngOnInit(): void { }

  login() {
    this.accountService.login(this.model).subscribe(response => {
      this.modalRef.hide();
      this.router.navigateByUrl('/members');
    })
  }

  logout() {
    this.accountService.logout();
    this.router.navigateByUrl('/');
  }

  search() {
    console.log(this.searchObj);
  }

  openModal(template: TemplateRef<any>) {
    let modalConfig = {
      backdrop: true,
      ignoreBackdropClick: false,
      class: 'modal-md'
    }
    this.modalRef = this.modalService.show(template, modalConfig);
  }

  addAlert(): void {
    this.alerts.push({
      type: 'custom',
      msg: `帳號或密碼錯誤，請重新登入喔!`,
      timeout: 2000
    });
  }

  closeAlert(dismissedAlert: AlertComponent): void {
    this.alerts = this.alerts.filter(alert => alert !== dismissedAlert);
  }
}
