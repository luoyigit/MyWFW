using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contact.Api.Models;
using MongoDB.Driver;

namespace Contact.Api.Data
{
    public class MongoContactApplyRequestRepository:IContactApplyRequestRepository
    {
        private readonly ContactContext _context;

        public MongoContactApplyRequestRepository(ContactContext context)
        {
            _context = context;
        }
        public async Task<bool> AddRequestAsync(ContactApplyRequest request, CancellationToken cancellationToken)
        {
            var IsHasRequested =
                await _context.ContactApplyRequests.CountDocumentsAsync(o =>
                    o.UserId == request.UserId && o.ApplierId == request.ApplierId, cancellationToken: cancellationToken) > 0;
            if (IsHasRequested)
            {
                var filter = Builders<ContactApplyRequest>.Filter.And(
                    Builders<ContactApplyRequest>.Filter.Eq(b => b.ApplierId, request.ApplierId),
                    Builders<ContactApplyRequest>.Filter.Eq(b => b.UserId, request.UserId));
                var update = Builders<ContactApplyRequest>.Update.Set(b => b.ApplyTime, DateTime.Now);
                var result =
                    await _context.ContactApplyRequests.UpdateOneAsync(filter, update, null, cancellationToken);
                return result.MatchedCount == result.ModifiedCount && result.ModifiedCount == 1;
            }
            await _context.ContactApplyRequests.InsertOneAsync(request,null,cancellationToken);
            return true;
        }

        public async Task<bool> ApprovalAsync(int userId, int applierId, CancellationToken cancellationToken)
        {
            var filter = Builders<ContactApplyRequest>.Filter.And(
                Builders<ContactApplyRequest>.Filter.Eq(b =>b.UserId,userId),
                Builders<ContactApplyRequest>.Filter.Eq(b =>b.ApplierId, applierId)
            );
            //更新添加好友申请，修改状态和添加处理时间
            var update = Builders<ContactApplyRequest>.Update
                .Set(b => b.Approvaled, 1)
                .Set(b => b.HandleTime, DateTime.Now);
            var result =await _context.ContactApplyRequests.UpdateOneAsync(filter, update, null, cancellationToken);

            return result.MatchedCount == result.ModifiedCount && result.ModifiedCount == 1;
        }

        public async Task<List<ContactApplyRequest>> GetRequestList(int userId, CancellationToken cancellationToken)
        {
            var requests = await _context.ContactApplyRequests.FindAsync(b => b.UserId == userId, cancellationToken: cancellationToken);
            return requests.ToList(cancellationToken);
        }
    }
}