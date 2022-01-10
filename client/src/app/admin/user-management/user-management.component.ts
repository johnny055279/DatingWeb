import { Component, OnInit } from '@angular/core';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { RolesModalComponent } from 'src/app/modals/roles-modal/roles-modal.component';
import { User } from 'src/app/_models/user';
import { AdminService } from 'src/app/_services/admin.service';

@Component({
  selector: 'app-user-management',
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.css']
})
export class UserManagementComponent implements OnInit {
  users: Partial<User[]>;
  bsModalRef: BsModalRef;
  constructor(private adminService: AdminService, private modalService: BsModalService) { }

  ngOnInit(): void {
    this.getUsersWithRoles();
  }

  getUsersWithRoles() {
    this.adminService.getUsersWithRoles().subscribe(user => {
      this.users = user;
    })
  }

  openRolesModal(user: User) {
    const config = {
      class: 'modal-dialog-center',
      initialState: {
        user,
        roles: this.getRolesArray(user)
      }
    }
    this.bsModalRef = this.modalService.show(RolesModalComponent, config);
    // BsModalRef.content is a reference to the component instance itself.
    // BsModalRef also takes a type parameter so we can specify BsModalRef<RolesModalComponent>
    // and get access to any of the properties on the instance.
    this.bsModalRef.content.updateSelectRoles.subscribe(values => {
      const rolesToUpdate = {
        // 允許可迭代的陣列或字串展開成０到多個參數(如果是function的話)或是０到多個元素(如果是array或字組的話)，
        // 或如果是物件的話則展開成０到多個key-value pair。
        roles: [...values.filter(n => n.checked === true).map(n => n.name)]
      };
      if (rolesToUpdate) {
        this.adminService.updateUserRoles(user.userName, rolesToUpdate.roles).subscribe((response) => {
          // 指定回user權限，就不用在call api重整
          user.roles = [...rolesToUpdate.roles];
        })
      }
    });

  }

  private getRolesArray(user: User) {
    const roles = [];
    const userRoles = user.roles;
    const availableRoles: any[] = [
      { name: 'Admin', value: 'Adimn' },
      { name: 'Moderator', value: 'Moderator' },
      { name: 'Member', value: 'Member' },
    ];

    availableRoles.forEach(role => {
      let isMatch = false;

      for (const userRole of userRoles) {
        if (role.name === userRole) {
          isMatch = true;
          role.checked = true;
          roles.push(role);
          break;
        }
      }
      if (!isMatch) {
        role.checked = false;
        roles.push(role);
      }
    });
    return roles;
  }

}
