using AutoMapper;
using DSAMate.API.Data.Domains;
using DSAMate.API.Models.Domains;
using DSAMate.API.Models.Dtos;

namespace DSAMate.API.Mappings
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<CreateQuestionDTO, Question>()
                .ForMember(dest => dest.Difficulty, 
                           opt => opt.MapFrom(src => Enum.Parse<Difficulty>(src.Difficulty)))
                .ForMember(dest => dest.Topic, opt => opt.MapFrom(src => Enum.Parse<Topic>(src.Topic)));
            CreateMap<Question, QuestionDTO>()
                .ForMember(dest => dest.Difficulty, opt => opt.MapFrom(src => src.Difficulty.ToString()))
                .ForMember(dest => dest.Topic, opt => opt.MapFrom(src => src.Topic.ToString()));
        }
    }
}
