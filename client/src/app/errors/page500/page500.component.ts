import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-page500',
  templateUrl: './page500.component.html',
  styleUrls: ['./page500.component.css']
})
export class Page500Component implements OnInit {

  error: any;

  // Router只被允許使用在constructor
  constructor(private router: Router, ) { 
    const navigation = this.router.getCurrentNavigation();

    // 因為router只會第一次才被觸發，因此當使用者重新整理頁面，就沒有資訊在裡面。
    // 所以可能是null。
    this.error = navigation?.extras?.state?.error; 
  }

  ngOnInit(): void {
  }

}
