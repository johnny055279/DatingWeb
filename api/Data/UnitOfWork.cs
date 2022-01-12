using System;
using System.Threading.Tasks;
using AutoMapper;
using Dating_WebAPI.Interfaces;

namespace Dating_WebAPI.Data
{
	public class UnitOfWork : IUnitOfWork
	{
        private readonly DataContext dataContext;

        private readonly IMapper mapper;

		public UnitOfWork(DataContext dataContext, IMapper mapper)
		{
            this.dataContext = dataContext;

            this.mapper = mapper;
		}

        public IUserRepository userRepository => new UserRepository(dataContext, mapper);

        public IMessageRepository messageRepository => new MessageRepository(dataContext, mapper);

        public ILikesRepository likesRepository => new LikesRepository(dataContext);

        public async Task<bool> Complete()
        {
            // SaveChangesAsync返回int
            // 因為是要返回布林值，所以當有儲存時就會 > 0
            return await dataContext.SaveChangesAsync() > 0;
        }

        public bool HasChanges()
        {
            return dataContext.ChangeTracker.HasChanges();
        }
    }
}

