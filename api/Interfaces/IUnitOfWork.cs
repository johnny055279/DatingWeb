using System;
using System.Threading.Tasks;

namespace Dating_WebAPI.Interfaces
{
	public interface IUnitOfWork
	{
		IUserRepository userRepository { get; }

		IMessageRepository messageRepository { get; }

		ILikesRepository likesRepository { get; }

		Task<bool> Complete();

        bool HasChanges();
    }
}

