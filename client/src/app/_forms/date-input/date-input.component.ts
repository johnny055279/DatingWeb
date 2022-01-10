import { Component, Input, OnInit, Self } from '@angular/core';
import { ControlValueAccessor, NgControl } from '@angular/forms';
import { BsDatepickerConfig } from 'ngx-bootstrap/datepicker';

@Component({
  selector: 'app-date-input',
  templateUrl: './date-input.component.html',
  styleUrls: ['./date-input.component.css']
})

export class DateInputComponent implements ControlValueAccessor {

  @Input() label!: string;
  @Input() maxDate!: Date;
  @Input() minDate!: Date;
  // Partial意思就是，任何在<>內的property，都是optional，而不用每個都定義。
  bsConfig!: Partial<BsDatepickerConfig>;

  constructor(@Self() public ngControl: NgControl) { 

    this.ngControl.valueAccessor = this;
    this.bsConfig = {
      containerClass: 'theme-dark-blue',
      dateInputFormat: 'DD MMMM YYYY',
      
    }

  }



  writeValue(obj: any): void {
  }
  registerOnChange(fn: any): void {
  }
  registerOnTouched(fn: any): void {
  }



}
