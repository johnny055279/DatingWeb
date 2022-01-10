import { Injectable } from '@angular/core';
import { CanDeactivate, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';
import { MemeberEditComponent } from '../members/memeber-edit/memeber-edit.component';

@Injectable({
  providedIn: 'root'
})
export class PreventUnsaveChangesGuard implements CanDeactivate<unknown> {
  canDeactivate(component: MemeberEditComponent): boolean {
      if(component.editForm.dirty){
        return confirm("確定要繼續嗎? 任何未經儲存的資料都會遺失。")
      }
    return true;
  }
  
}
