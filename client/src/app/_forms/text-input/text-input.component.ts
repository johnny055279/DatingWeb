import { Component, Input, OnInit, Self } from '@angular/core';
import { ControlValueAccessor, NgControl } from '@angular/forms';

@Component({
  selector: 'app-text-input',
  templateUrl: './text-input.component.html',
  styleUrls: ['./text-input.component.css']
})

// 若要使用FormControl，這裡不使用OnInit而是ControlValueAccessor
// ControlValueAccessor是Angular API與DOM之間的溝通者。
export class TextInputComponent implements ControlValueAccessor {

  @Input() label!: string;
  @Input() type!: 'text';
  
  // 使用 @Self 裝飾器時，注入器只在該元件的注入器中查詢提供者。
  constructor(@Self() public ngControl: NgControl) 
  {
    this.ngControl.valueAccessor = this;
  }

  //實作介面
  writeValue(obj: any): void {
  }
  registerOnChange(fn: any): void {
  }
  registerOnTouched(fn: any): void {
  }

  

}
