import { HttpClient, HttpParams} from "@angular/common/http";
import { map } from "rxjs/operators";
import { PaginatedResult } from "../_models/pagination";

export function getPaginationResult<T>(url: string, params: HttpParams, http: HttpClient) {

    const paginatedResult: PaginatedResult<T> = new PaginatedResult<T>();

    return http.get<T>(url, { observe: 'response', params }).pipe(
      map(response => {
        // member array會塞在body
        paginatedResult.result = response.body;

        // 取得由API提供的Header
        if (response.headers.get('Pagination') !== null) {
          paginatedResult.pagination = JSON.parse(response.headers.get('Pagination'));
        }
        return paginatedResult;
    }));
}

export function  getPaginationHeaders(pageNumber: number, pageSize: number){

    // HttpParams可以序列化我們的QueryString
    let params = new HttpParams();

    // 因為是要傳QueryString，所以要toString
    params = params.append('pageNumber', pageNumber.toString());
    params = params.append('pageSize', pageSize.toString());
  
    return params;
}