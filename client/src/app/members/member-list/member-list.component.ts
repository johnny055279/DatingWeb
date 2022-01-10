import { Component, OnInit } from '@angular/core';
import { Member } from 'src/app/_models/member';
import { Pagination } from 'src/app/_models/pagination';
import { User } from 'src/app/_models/user';
import { UserParams } from 'src/app/_models/userParams';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {

  members!: Member[];
  pagination: Pagination;
  userParams: UserParams;
  user: User;
  genderList = [{value: 'male', display: '男性'},{value: 'female', display: '女性'}];

  constructor(private memberServices: MembersService) { 
    this.userParams = this.memberServices.getUSerParams();
  }

  ngOnInit(): void {

    this.loadMembers();

  }

  loadMembers(){

    this.memberServices.setUserParams(this.userParams);

    // 這裡是使用HttpResponse去取得資料，所以不用pipe
    this.memberServices.getMembers(this.userParams).subscribe(response=>{
      this.members = response.result;
      this.pagination = response.pagination;
    })
  }

  pageChange(event: any){
    this.userParams.pageNumber = event.page;
    this.memberServices.setUserParams(this.userParams);
    this.loadMembers();
  }

  resetFilters(){
    this.userParams = this.memberServices.reSetUserParams();
    this.loadMembers();
  }

}
