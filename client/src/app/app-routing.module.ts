import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AdminPanelComponent } from './admin/admin-panel/admin-panel.component';
import { ErrorsComponent } from './errors/errors/errors.component';
import { Page404Component } from './errors/page404/page404.component';
import { Page500Component } from './errors/page500/page500.component';
import { HomeComponent } from './home/home.component';
import { ListsComponent } from './lists/lists.component';
import { MemberDetailComponent } from './members/member-detail/member-detail.component';
import { MemberListComponent } from './members/member-list/member-list.component';
import { MemeberEditComponent } from './members/memeber-edit/memeber-edit.component';
import { MessagesComponent } from './messages/messages.component';
import { AdminGuard } from './_guards/admin.guard';
import { AuthGuard } from './_guards/auth.guard';
import { PreventUnsaveChangesGuard } from './_guards/prevent-unsave-changes.guard';
import { MemberDetailResolver } from './_resolvers/member-detailed.resolver';

const routes: Routes = [
  { path: '', component: HomeComponent },
  {
    path: '',
    runGuardsAndResolvers: 'always',
    canActivate: [AuthGuard],
    // 使用children可以統一規則，例如AuthGuard
    children: [
      { path: 'members', component: MemberListComponent },
      { path: 'members/:username', component: MemberDetailComponent, resolve: { member: MemberDetailResolver } },
      { path: 'member/edit', component: MemeberEditComponent, canDeactivate: [PreventUnsaveChangesGuard] },
      { path: 'lists', component: ListsComponent },
      { path: 'messages', component: MessagesComponent },
      { path: 'admin', component: AdminPanelComponent, canActivate: [AdminGuard] },
    ]
  },
  { path: 'errors', component: ErrorsComponent },
  { path: 'notFound', component: Page404Component },
  { path: 'serverError', component: Page500Component },
  // pathMath意味著是否符合其Url匹配，full代表所有規則都要符合。
  { path: '**', component: Page404Component, pathMatch: 'full' },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
