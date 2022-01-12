using AutoMapper;
using Dating_WebAPI.DTOs;
using Dating_WebAPI.Entities;
using Dating_WebAPI.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dating_WebAPI.Helpers
{
    // 下載AutoMapper dependency injection套件並繼承Profile
    // AutoMapper的功用：
    // 1. 定義好兩個Class之間轉換的邏輯
    // 2. 把object透過AutoMapper轉換成為另外一個形態的object
    // 3. 在service注入才可使用。
    // 這樣有助於EF對應ViewModel
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            // ForMember代表設定哪個屬性我們想要控制
            // 第一個參數是我們想控制的屬性，第二個參數擺我們要Mapping的過去的地方
            CreateMap<AppUser, MemberDTO>().ForMember(
                destination => destination.PhotoUrl,
                option => option.MapFrom(
                    src => src.Photos.FirstOrDefault(n => n.IsMain).Url))
                .ForMember(destination => destination.Age, option => option.MapFrom(n => n.Birthday.CalculateAge()));

            CreateMap<Photo, PhotoDTO>();

            CreateMap<MemberUpdateDTO, AppUser>();

            CreateMap<RegisterDTO, AppUser>();

            CreateMap<Message, MessageDto>()
                .ForMember(destination => destination.SenderPhotoUrl,
                                                       option => option.MapFrom(
                                                       src => src.Sender.Photos.FirstOrDefault(
                                                       n => n.IsMain).Url))
                .ForMember(destination => destination.RecipientPhotoUrl,
                                                       option => option.MapFrom(
                                                       src => src.Recipient.Photos.FirstOrDefault(
                                                       n => n.IsMain).Url));
        }
    }
}