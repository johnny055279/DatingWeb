import { Component, EventEmitter, Input, OnInit } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { User } from 'src/app/_models/user';

@Component({
  selector: 'app-roles-modal',
  templateUrl: './roles-modal.component.html',
  styleUrls: ['./roles-modal.component.css']
})
export class RolesModalComponent implements OnInit {

  constructor(public bsModalRef: BsModalRef) { }
  @Input() updateSelectRoles = new EventEmitter();
  user: User;
  roles: any[];
  ngOnInit(): void {
  }

  updateRoles() {
    this.updateSelectRoles.emit(this.roles);
    this.bsModalRef.hide();
  }

}
