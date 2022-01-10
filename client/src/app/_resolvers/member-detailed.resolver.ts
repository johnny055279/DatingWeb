import { Injectable } from "@angular/core";
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from "@angular/router";
import { Observable } from "rxjs";
import { Member } from "../_models/member";
import { MembersService } from "../_services/members.service";

@Injectable({
    providedIn: 'root'
})

export class MemberDetailResolver implements Resolve<Member>{


    constructor(private memberService: MembersService){}

    resolve(route: ActivatedRouteSnapshot): Observable<Member> | Promise<Member> {

        // 使頁面在construct之前先得到data
        return this.memberService.getMember(route.paramMap.get('username'));
    }

}