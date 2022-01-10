import { Photo } from "./photo";

export interface Member {
    userId: number;
    gender: string;
    userName: string;
    email: string;
    nickName: string;
    age: number;
    accountCreateTime: Date;
    lastLoginTime: Date;
    lookingFor: string;
    interests: string;
    city: string;
    country: string;
    introduction: string;
    photos: Photo[];
    photoUrl: string;
}
