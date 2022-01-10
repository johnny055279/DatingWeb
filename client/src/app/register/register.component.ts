import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { AbstractControl, FormBuilder, FormControl, FormGroup, ValidatorFn, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  registerForm!: FormGroup;
  maxDate!:Date;
  validationErrors: string[] = [];

  // 輸入屬性，通常為接收數據資料，也就是就是讓Parent將資料傳送到Child中使用。寫在Child裡面。
  @Input() FromHomeComponent: any = {};
  // 輸出屬性，通常提供事件給外部呼叫回傳使用，也就是讓Child將資料傳回Parent中使用。寫在Child裡面。
  @Output() cancelRegister = new EventEmitter();

  constructor(private accountService: AccountService, 
              private toastr: ToastrService, 
              private fb: FormBuilder,
              private router: Router){}

  ngOnInit(): void {
    this.initializeForm();
    this.maxDate = new Date();
    this.maxDate.setFullYear(this.maxDate.getFullYear() - 18);
  }

  initializeForm(){
    this.registerForm = this.fb.group({
      userName: ['', Validators.required],
      password: ['', [Validators.required, Validators.minLength(4), Validators.maxLength(8)]],
      // 自訂義Validation
      confirmPassword: ['', [Validators.required, this.matchValues('password')]],
      email: ['', Validators.required],
      gender: ['male'],
      birthday: ['', Validators.required],
      nickName: ['', Validators.required],
      lookingFor: ['', Validators.required],
      interests: ['', Validators.required],
      city: ['', Validators.required],
      country: ['', Validators.required],
      introduction: ['', Validators.required]
    });


    // 避免自訂義的Validation只針對confirmPassword有作用，而當變更password時會出現判斷錯誤
    // 因此要進行更新。
    this.registerForm.controls.password.valueChanges.subscribe(() => {
      this.registerForm.controls.confirmPassword.updateValueAndValidity();
    })


  }

  // 自訂義Validation
  matchValues(matchTo: string): ValidatorFn {

    // 所有的FormControl都源於自AbstractControl
    return (control: AbstractControl) => {
      if (control.parent && control.parent.controls) {
         // 也要定義要比對的control的index的型別。
        return control?.value === (control?.parent?.controls as { [key: string]: AbstractControl })[matchTo].value ? null : {isMatching: true}
      }
      return null;
    }
  }


  register(){
    this.accountService.register(this.registerForm.value).subscribe(() => 
    {
      this.router.navigateByUrl('/members');
      this.toastr.success("註冊成功!");
      this.cancel();
    },
    error => {
      this.validationErrors = error;
    })}

  cancel(){
    this.cancelRegister.emit(false);
  }
  
}
