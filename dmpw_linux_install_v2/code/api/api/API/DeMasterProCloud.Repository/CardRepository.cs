using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.Card;
using DeMasterProCloud.DataModel.User;
using Newtonsoft.Json;

namespace DeMasterProCloud.Repository
{
    /// <summary>
    /// User repository interface
    /// </summary>
    public interface ICardRepository : IGenericRepository<Card>
    {

        IQueryable<Card> GetByCompanyId(int companyId);
        Card GetById(int companyId, int id);
        List<Card> GetByIds(int companyId, List<int> ids);

        List<Card> GetByUserId(int companyId, int userId);
        List<Card> GetByVisitId(int companyId, int visitId);
        List<Card> GetByVisitIds(int companyId, List<int> visitIds);
        Card GetByVisitId(int visitId);

        int GetCountByUserId(int companyId, int userId);

       // List<Card> GetByUserIdIncludeUser(int companyId, int userId);
        bool IsCardIdExist(int companyId, List<CardModel> cardList);
        
        bool IsCardIdByUserIdExist(int companyId, int userId, int cardId);
        bool IsCardIdExist(int companyId, string cardId);
        bool IsCardExist(short type, int userId);
        bool IsCardVisitExist(short type, int visitId);
        List<int> GetUserIdHasCards(int companyId);
        List<int> GetVisitIdHasCards(int companyId);

        IQueryable<Card> GetFilteredCards(int companyId, string filter);
        Card GetByCardId(int? companyId, string cardId);
        int GetCardCountByuserIds(int companyId, List<int> userIds);
        int GetCardStatusByUserIdAndCardId(int companyId, int userId,  string cardId);
        bool CheckIsExistedQrCode(string qrId);
        
        Card GetQrCode(int userId);
        Card GetQrCodeForVisitor(int visitorId);
        Card GetNFCPhoneId(int userId);
        Card GetRandomQrCode(int companyId);
        List<Card> GetCardQrByCompanyId(int companyId);
        
        Card GetPassCode(int userId, int companyId);
        List<Card> GetQrCodeByUsers(List<User> users);
        List<Card> GetNFCPhoneByUsers(List<User> users);
        List<Card> GetFaceIdByUsers(List<User> users);
        Card GetHFaceIdForVisitor(int visitorId);
        Card GetHFaceIdForUser(int userId);

        IQueryable<Card> GetListCardByRoleAndCompany(int cardRole, int companyId);
        void SetFingerPrintToCard(int cardId, List<FingerPrintModel> data);
        IQueryable<FingerPrint> GetFingerPrintByCard(int cardId);
        IQueryable<Card> GetCardAvailableInDevice(int deviceId);
        IQueryable<Card> GetCardDetailByIds(List<int> ids);
        void DeleteFacesByUserId(int userId);
        Card GetEbknFaceByUserId(int userId);
        Card GetEbknFingerprintByUserId(int userId);
        IQueryable<Face> GetFacesByUserId(int userId);
    }

    /// <summary>
    /// Card repository
    /// </summary>
    public class CardRepository : GenericRepository<Card>, ICardRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger _logger;
        public CardRepository(AppDbContext dbContext, IHttpContextAccessor contextAccessor) : base(dbContext,
            contextAccessor)
        {
            _dbContext = dbContext;
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<CardRepository>();
        }

        /// <summary>
        /// Get by company ID
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public IQueryable<Card> GetByCompanyId(int companyId)
        {
            return _dbContext.Card
                .Where(m => m.CompanyId == companyId && !m.IsDeleted);
        }

        /// <summary>
        /// Get by companyid and id
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public Card GetById(int companyId, int id)
        {
            return _dbContext.Card.Where(m =>
                m.Id == id && m.CompanyId == companyId && !m.IsDeleted).FirstOrDefault();
        }


        public List<Card> GetByIds(int companyId, List<int> ids)
        {
            return _dbContext.Card.Where(m =>
                ids.Contains(m.Id) && m.CompanyId == companyId && !m.IsDeleted).ToList();
        }

        /// <summary>
        /// Check whether if card id is existed in the company
        /// </summary>
        /// <param name="model"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public bool IsCardIdExist(int companyId, List<CardModel> cardList)
        {
            if (!cardList.Any())
            {
                return false;
            }

            return Get(m =>
                       cardList.Select(card => card.CardId).Contains(m.CardId) &&
                       m.CompanyId == companyId && !m.IsDeleted) != null;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="userId">index number of user</param>
        /// <param name="cardId">index number of card, not cardId</param>
        /// <returns></returns>
        public bool IsCardIdByUserIdExist(int companyId, int userId, int cardId)
        {
            return Get(m => m.Id == cardId && m.CompanyId == companyId && !m.IsDeleted && m.UserId == userId) !=
                   null;
        }
        
        /// <summary>
        /// Check whether if card id is existed in the company
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="cardId"></param>
        /// <returns></returns>
        public bool IsCardIdExist(int companyId, string cardId)
        {
            if (string.IsNullOrEmpty(cardId))
            {
                return false;
            }

            return Get(m => m.CardId == cardId && m.CompanyId == companyId && !m.IsDeleted) != null;
        }

        /// <summary>
        /// Check cardId (with type) exist by userId
        /// </summary>
        /// <param name="type"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool IsCardExist(short type, int userId)
        {
            return GetMany(m => m.CardType == type && m.UserId == userId && !m.IsDeleted).ToList().Count != 0;
        }
        
        public bool IsCardVisitExist(short type, int visitId)
        {
            return GetMany(m => m.CardType == type && m.VisitId == visitId && !m.IsDeleted).ToList().Count != 0;
        }
        
        /// <summary>
        /// Get list of card by user id
        /// </summary>
        /// <param name="companyId"> identifier of company </param>
        /// <param name="userId"> identifier of user </param>
        /// <returns></returns>
        public List<Card> GetByUserId(int companyId, int userId)
        {
            var data = _dbContext.Card.Where(m => m.UserId == userId && !m.IsDeleted);

            if(companyId != 0)
            {
                data = data.Where(m => m.CompanyId == companyId);
            }

            return data.ToList();
        }

        /// <summary>
        /// Get list of card by visit id
        /// </summary>
        /// <param name="companyId"> identifier of company </param>
        /// <param name="visitId"> identifier of visit </param>
        /// <returns></returns>
        public List<Card> GetByVisitId(int companyId, int visitId)
        {
            var data = _dbContext.Card.Where(m => m.VisitId == visitId && !m.IsDeleted);

            if (companyId != 0)
            {
                data = data.Where(m => m.CompanyId == companyId);
            }

            return data.ToList();
        }

        public List<Card> GetByVisitIds(int companyId, List<int> visitIds)
        {
            var data = _dbContext.Card.Where(m => m.VisitId != null && visitIds.Contains(m.VisitId ?? 0) && !m.IsDeleted);

            if (companyId != 0)
            {
                data = data.Where(m => m.CompanyId == companyId);
            }

            return data.ToList();
        }

        public Card GetByVisitId(int visitId)
        {
            var data = _dbContext.Card.FirstOrDefault(m => m.VisitId == visitId && !m.IsDeleted 
                && m.CardType != (int)CardType.VehicleId && m.CardType != (short)CardType.VehicleMotoBikeId);

            return data;
        }

        public int GetCountByUserId(int companyId, int userId)
        {
            var data = _dbContext.Card.Where(m => m.UserId == userId && m.CompanyId == companyId && !m.IsDeleted);

            if(companyId != 0)
            {
                data = data.Where(m => m.CompanyId == companyId);
            }

            //return _dbContext.Card.Where(m =>
            //    m.UserId == userId && m.CompanyId == companyId && !m.IsDeleted).Count();

            return data.Count();
        }

        //public List<Card> GetByUserIdIncludeUser(int companyId, int userId)
        //{
        //    return _dbContext.Card.Include(c => c.User).Where(m =>
        //        m.UserId == userId && m.CompanyId == companyId && !m.IsDeleted).ToList();
        //}

        public List<int> GetUserIdHasCards(int companyId)
        {
            return _dbContext.Card.Where(m =>
                m.CompanyId == companyId && !m.IsDeleted && m.UserId != null).Select(c => c.UserId.Value).ToList();
        }

        public List<int> GetVisitIdHasCards(int companyId)
        {
            return _dbContext.Card.Where(m => m.CompanyId == companyId && !m.IsDeleted && m.VisitId != null).Select(c => c.VisitId.Value).ToList();
        }

        public IQueryable<Card> GetFilteredCards(int companyId, string filter)
        {
            return _dbContext.Card.Where(m => m.CardId.Contains(filter));
        }

        public Card GetByCardId(int? companyId, string cardId)
        {
            return _dbContext.Card.FirstOrDefault(m => m.CardId.ToLower().Equals(cardId.ToLower()) && m.CompanyId == companyId && !m.IsDeleted && (m.UserId.HasValue || m.VisitId.HasValue));
        }

        public int GetCardStatusByUserIdAndCardId(int companyId, int userId, string cardId)
        {
            return _dbContext.Card.Where(m =>
                m.CardId.Equals(cardId) && m.UserId == userId && m.CompanyId == companyId && !m.IsDeleted).Select(m => m.CardStatus).FirstOrDefault();
        }
        public int GetCardCountByuserIds(int companyId, List<int> userIds)
        {
            return _dbContext.Card.Where(m => m.UserId != null &&
                userIds.Contains(m.UserId.Value) && m.CompanyId == companyId && !m.IsDeleted).Count();
        }

        public bool CheckIsExistedQrCode(string qrId)
        {
            var qr = _dbContext.Card.Where(m => !m.IsDeleted && m.CardId.ToLower() == qrId.ToLower()).Count();
            if (qr != 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        
        public List<Card> GetCardQrByCompanyId(int companyId)
        {
            return GetMany(m => m.CompanyId == companyId && m.CardType == 2).ToList();
        }
        
        public Card GetQrCode(int userId)
        {
            var qr = _dbContext.Card.FirstOrDefault(m =>
                !m.IsDeleted && m.CardType == (short) CardType.QrCode && m.UserId == userId);
            return qr;
        }

        public Card GetQrCodeForVisitor(int visitorId)
        {
            var qr = _dbContext.Card.FirstOrDefault(m =>
                !m.IsDeleted && m.CardType == (short) CardType.QrCode && m.VisitId == visitorId);
            return qr;
        }

        public Card GetNFCPhoneId(int userId)
        {
            var qr = _dbContext.Card.FirstOrDefault(m =>
                !m.IsDeleted && m.CardType == (short)CardType.NFCPhone && m.UserId == userId);
            return qr;
        }
        
        public Card GetRandomQrCode(int companyId)
        {
            return _dbContext.Card.FirstOrDefault(m => !m.IsDeleted && m.CardType == (short) CardType.QrCode && m.CompanyId == companyId);
        }

        public Card GetPassCode(int userId, int companyId)
        {
            return _dbContext.Card.FirstOrDefault(m => !m.IsDeleted && m.CardType == (short) CardType.PassCode && m.CompanyId == companyId && m.UserId == userId);
        }

        public List<Card> GetQrCodeByUsers(List<User> users)
        {
            List<int> userIds = new List<int>();
            foreach(User user in users)
            {
                userIds.Add(user.Id);
            }
            return _dbContext.Card.Where(c => userIds.Contains((int)c.UserId) && c.CardType==(short) CardType.QrCode && c.IsDeleted == false).ToList();
        }

        public List<Card> GetNFCPhoneByUsers(List<User> users)
        {
            List<int> userIds = new List<int>();
            foreach (User user in users)
            {
                userIds.Add(user.Id);
            }
            return _dbContext.Card.Where(c => userIds.Contains((int)c.UserId) && c.CardType == (short)CardType.NFCPhone && c.IsDeleted == false).ToList();
        }

        public List<Card> GetFaceIdByUsers(List<User> users)
        {
            List<int> userIds = new List<int>();
            foreach (User user in users)
            {
                userIds.Add(user.Id);
            }
            return _dbContext.Card.Where(c => userIds.Contains((int)c.UserId) && c.CardType == (short)CardType.FaceId && c.IsDeleted == false).ToList();
        }

        public Card GetHFaceIdForVisitor(int visitorId)
        {
            return _dbContext.Card.FirstOrDefault(m => !m.IsDeleted && m.VisitId == visitorId && m.CardType == (short)CardType.HFaceId);
        }

        public Card GetHFaceIdForUser(int userId)
        {
            return _dbContext.Card.FirstOrDefault(m => !m.IsDeleted && m.UserId == userId && m.CardType == (short)CardType.HFaceId);
        }
        
        public IQueryable<Card> GetListCardByRoleAndCompany(int cardRole, int companyId)
        {
            return _dbContext.Card.Include(m => m.User).ThenInclude(u => u.Department)
                .Include(m => m.Visit)
                .Where(m => !m.IsDeleted && m.CompanyId == companyId && m.CardRole == cardRole);
        }

        public void SetFingerPrintToCard(int cardId, List<FingerPrintModel> data)
        {
            var fingerprints = _dbContext.FingerPrint.AsNoTracking().Where(m => m.CardId == cardId).ToList();
            IEnumerable<FingerPrint> fingerprintsDeleted = fingerprints;

            foreach (var item in data)
            {
                FingerPrint fingerPrint = null;
                if (item.Id != 0)
                {
                    fingerPrint = fingerprints.FirstOrDefault(m => m.Id == item.Id);
                }

                if (fingerPrint == null)
                {
                    var newFingerPrint = new FingerPrint()
                    {
                        Note = item.Note,
                        Templates = JsonConvert.SerializeObject(item.Templates ?? new List<string>()),
                        CardId = cardId,
                        Id = 0,
                    };
                    try
                    {
                        _dbContext.FingerPrint.Add(newFingerPrint);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in SetFingerPrintToCard");
                    }
                }
                else
                {
                    fingerPrint.Note = item.Note;
                    fingerPrint.Templates = JsonConvert.SerializeObject(item.Templates ?? new List<string>());
                    fingerPrint.CardId = cardId;
                    fingerPrint.Id = item.Id;
                    try
                    {
                        _dbContext.FingerPrint.Update(fingerPrint);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in SetFingerPrintToCard");
                    }
                    fingerprintsDeleted = fingerprintsDeleted.Where(m => m.Id != item.Id).ToList();
                }
            }

            if (fingerprintsDeleted.Any())
            {
                try
                {
                    _dbContext.FingerPrint.RemoveRange(fingerprintsDeleted);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in SetFingerPrintToCard");
                }
            }

            try
            {
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SetFingerPrintToCard");
            }
        }

        public IQueryable<FingerPrint> GetFingerPrintByCard(int cardId)
        {
            return _dbContext.FingerPrint.Where(m => m.CardId == cardId);
        }

        public IQueryable<Card> GetCardAvailableInDevice(int deviceId)
        {
            var device = _dbContext.IcuDevice.FirstOrDefault(m => !m.IsDeleted && m.Id == deviceId);
            if (device != null && device.CompanyId.HasValue)
            {
                // list filter status
                List<short> cardStatus = Helpers.GetCardStatusToSend();
                List<int> cardType = Helpers.GetMatchedIdentificationType(device.DeviceType);
                List<int> userStatus = new List<int>()
                {
                    (short)ApprovalStatus.Approved,
                    (short)ApprovalStatus.NotUse,
                };
                List<short> visitStatus = new List<short>()
                {
                    (short)VisitChangeStatusType.Approved,
                    (short)VisitChangeStatusType.CardIssued,
                };
                
                // DateTime start = DateTime.UtcNow;
                
                var data = _dbContext.Card
                    .Include(m => m.User)
                    .Include(m => m.User).ThenInclude(m => m.Department)
                    .Include(m => m.User).ThenInclude(m => m.Face)
                    .Include(m => m.Visit)
                    .Include(m => m.FingerPrint)
                    .Where(m => !m.IsDeleted && !string.IsNullOrWhiteSpace(m.CardId) && m.CompanyId == device.CompanyId && cardStatus.Contains(m.CardStatus) && cardType.Contains(m.CardType))
                    .Where(m => (m.UserId.HasValue && !m.User.IsDeleted && m.User.Status == (short)Status.Valid && userStatus.Contains(m.User.ApprovalStatus) && (m.User.AccessGroup.AccessGroupDevice.Any(n => n.IcuId == device.Id) || m.User.AccessGroup.Parent.AccessGroupDevice.Any(n => n.IcuId == device.Id)))
                                || (m.VisitId.HasValue && !m.IsDeleted && visitStatus.Contains(m.Visit.Status) && m.Visit.EndDate > DateTime.UtcNow && m.Visit.AccessGroup.AccessGroupDevice.Any(n => n.IcuId == device.Id))
                                );
                
                // DateTime end = DateTime.UtcNow;
                // Console.WriteLine($"[GET CARDS AVAILABLE IN DEVICE {device.DeviceAddress}]: Total Cards = {data.Count()}, Total Time = {end.Subtract(start).TotalMilliseconds} (ms)");
                return data;
            }
            
            return new EnumerableQuery<Card>(new List<Card>());
        }

        public IQueryable<Card> GetCardDetailByIds(List<int> ids)
        {
            return _dbContext.Card.Where(m => ids.Contains(m.Id))
                .Include(m => m.User)
                .Include(m => m.User).ThenInclude(m => m.Department)
                .Include(m => m.User).ThenInclude(m => m.Face)
                .Include(m => m.Visit)
                .Include(m => m.FingerPrint);
        }
        
        public void DeleteFacesByUserId(int userId)
        {
            var faces = _dbContext.Face.Where(m => m.UserId == userId);
            try
            {
                _dbContext.Face.RemoveRange(faces);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteFacesByUserId");
            }
        }

        public Card GetEbknFaceByUserId(int userId)
        {
            return _dbContext.Card.FirstOrDefault(m => m.UserId == userId && !m.IsDeleted && m.CardType == (short)CardType.EbknFaceId);
        }

        public Card GetEbknFingerprintByUserId(int userId)
        {
            return _dbContext.Card.FirstOrDefault(m => m.UserId == userId && !m.IsDeleted && m.CardType == (short)CardType.EbknFingerprint);
        }

        public IQueryable<Face> GetFacesByUserId(int userId)
        {
            return _dbContext.Face.Where(m => m.UserId == userId);
        }
    }
}