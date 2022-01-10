import { Component, Input, OnInit } from '@angular/core';
import { FileUploader } from 'ng2-file-upload';
import { take } from 'rxjs/operators';
import { Member } from 'src/app/_models/member';
import { Photo } from 'src/app/_models/photo';
import { User } from 'src/app/_models/user';
import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from 'src/app/_services/members.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-photo-editor',
  templateUrl: './photo-editor.component.html',
  styleUrls: ['./photo-editor.component.css']
})
export class PhotoEditorComponent implements OnInit {

  @Input() member!: Member;
  uploader!:FileUploader;
  hasBaseDropZonOver = false;
  baseUrl = environment.apiUrl;
  user!: User;
  constructor(private accountService: AccountService, private memberService: MembersService) {
    this.accountService.currentUser$.pipe(take(1)).subscribe(user => this.user = user);
   }

  ngOnInit(): void {
    this.initializeUploader();
  }

  fileOverBase(event: any){
    this.hasBaseDropZonOver = event;
  }

  // 初始化套件
  initializeUploader(){
    this.uploader = new FileUploader({
      url: this.baseUrl + "users/addPhoto",
      authToken: "Bearer " + this.user.token,
      isHTML5: true,
      allowedFileType: ['image'],
      removeAfterUpload: true,
      autoUpload: false,
      maxFileSize: 10 * 1024 * 1024
    });

    this.uploader.onAfterAddingFile = (file) => {

      // 已經用token了，所以這裡不需要額外設定驗證。
      file.withCredentials = false;
    }

    this.uploader.onSuccessItem = (item, response, status, headers) => {
      if(response){
        const photo: Photo = JSON.parse(response);
        this.member.photos.push(photo);
        if(photo.isMain){
          this.user.photoUrl = photo.url;
          this.member.photoUrl = photo.url;
          this.accountService.setCurrentUser(this.user);
        }
      }
    }
  }

  
  setMainPhoto(photo: Photo){
    this.memberService.setMainPhoto(photo.id).subscribe(()=>{
      this.user.photoUrl = photo.url;
      // 更新user資訊
      this.accountService.setCurrentUser(this.user);
      this.member.photoUrl = photo.url;
      // 判斷每一張照片是不是main
      this.member.photos.forEach(n=>{
        if(n.isMain) n.isMain = false;
        if(n.id === photo.id) n.isMain = true;
      })
    });
  }

  deletePhoto(photoId: number){
    this.memberService.deletePhoto(photoId).subscribe(()=>{

      // filter 可以找出所有符合條件的元素
      this.member.photos = this.member.photos.filter(n => n.id !== photoId);
      

    });
  }
}
