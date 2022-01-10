import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NgxGalleryAnimation, NgxGalleryImage, NgxGalleryOptions } from '@kolkov/ngx-gallery';
import { Member } from 'src/app/_models/member';
import { strings as stringsTW } from 'ngx-timeago/language-strings/zh-TW';
import { TimeagoIntl } from 'ngx-timeago';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { Message } from 'src/app/_models/message';
import { MessageService } from 'src/app/_services/message.service';
import { PresenceService } from 'src/app/_services/presence.service';
import { AccountService } from 'src/app/_services/account.service';
import { take } from 'rxjs/operators';
import { User } from 'src/app/_models/user';


@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css'],
})
export class MemberDetailComponent implements OnInit, OnDestroy {

  // static當設定為true時，只會在ngOnInit的地方取得Element物件。
  // 不用再加上ngIf的判斷，因為在ngOnInit已經執行一次，ngIf依舊取得不到(ngAfterViewInit)。
  @ViewChild('memberTabs', { static: true }) memberTabs: TabsetComponent;
  activeTab: TabDirective;
  galleryOptions: NgxGalleryOptions[] = [];
  galleryImages: NgxGalleryImage[] = [];
  member!: Member;
  messages: Message[] = [];
  user: User;
  // 使用ActivatedRoute介面，就可以在Routing時將參數帶入。
  constructor(private route: ActivatedRoute,
    private intl: TimeagoIntl,
    private messageService: MessageService,
    public presence: PresenceService,
    public accountService: AccountService,
    private router: Router) {
    intl.strings = stringsTW;
    intl.changes.next();
    this.accountService.currentUser$.pipe(take(1)).subscribe((user => {
      this.user = user;

      // 告訴Angular不要reuse，避免聊天訊息沒辦法重新讀取
      this.router.routeReuseStrategy.shouldReuseRoute = () => false;
    }))
  }

  ngOnDestroy(): void {
    this.messageService.stopHubConnection();
  }

  ngOnInit(): void {

    this.route.data.subscribe(response => {
      this.member = response.member;
    });

    this.route.queryParams.subscribe(params => {
      params.tab ? this.selectTab(params.tab) : this.selectTab(0);
    })

    this.galleryOptions = [{
      width: '500px',
      height: '500px',
      imagePercent: 100,
      thumbnailsColumns: 4,
      imageAnimation: NgxGalleryAnimation.Slide,
      preview: false
    }]

    // 有route resolver就不必擔心在顯示頁面時資料還沒有回來造成undefine。
    this.galleryImages = this.getImages();
  }

  getImages(): NgxGalleryImage[] {
    let imageUrls = [];
    for (let photo of this.member.photos) {
      imageUrls.push({
        small: photo?.url,
        medium: photo?.url,
        big: photo?.url
      })
    }
    return imageUrls;
  }

  loadMessages() {
    this.messageService.getMessageThread(this.member.userName).subscribe(response => {
      this.messages = response;
    })
  }

  onTabActivate(data: TabDirective) {
    this.activeTab = data;
    if (this.activeTab.heading === '訊息' && this.messages.length === 0) {
      this.messageService.createHubConnection(this.user, this.member.userName)
    }
    else {
      this.messageService.stopHubConnection();
    }
  }

  selectTab(tabId: number) {
    this.memberTabs.tabs[tabId].active = true;
  }

}
