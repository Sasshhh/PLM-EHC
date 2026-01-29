using C8.eServices.Mvc.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class Comment : BaseModel
    {
        [Column(Order = 49)]
        [Display(Name = "Reference Type")]
        public int? ReferenceTypeId { get; set; }
        [ForeignKey("ReferenceTypeId")]
        public ReferenceType ReferenceType { get; set; }

        [Column(Order = 50)]
        [Display(Name = "Sender Customer Id")]
        public int? SenderCustomerId { get; set; }
        [ForeignKey("SenderCustomerId")]
        public Customer SenderCustomer { get; set; }



        [Column(Order = 51)]
        [Display(Name = "Numeric Reference")]
        public int ReferenceNumeric { get; set; }

        [Column(Order = 52)]
        [Display(Name = "Account Reference")]
        public string ReferenceAlpha { get; set; }

        [Column(Order = 53)]
        [Display(Name = "Comment")]
        public string Comments { get; set; }

        [Column(Order = 54)]
        [Display(Name = "CommunicationId")]
        public int? CommunicationId { get; set; }
        [ForeignKey("CommunicationId")]
        public Communication Communication { get; set; }



        #region Business Logic Methods

        readonly eServicesDbContext dbContext = new eServicesDbContext();
        public List<Comment> FindDepartmentWithComments(int RCSApplicationId)
        {
            //GetAll CommunicationTypes
            var department = new CommunicationType();
            //var departmentCommunicationTypes = department.GetAllCommunicationActive();
            var departmentCommunicationTypes = dbContext.CommunicationTypes.Where(o => o.IsActive && !o.IsDeleted).ToList();

            //Get the current conversation
            var CommuniicationBuilder = dbContext.Communications.ToList();
            var Conversation = CommuniicationBuilder.Where(x => x.RCSApplicationStatusId == RCSApplicationId).FirstOrDefault();

            //Get a list of comments for that conversation
            //var GetComments = dbContext.Comments.Where(x => x.CommunicationId == Conversation.Id).Include(c => c.Communication).Include(c => c.CreatedBySystemUser).Include(c => c.ModifiedBySystemUser).Include(c => c.ReferenceType).Include(c => c.SenderCustomer).Include(c => c.Communication).Include(c => c.CreatedBySystemUser).Include(c => c.ModifiedBySystemUser).Include(c => c.ReferenceType).Include(c => c.SenderCustomer).ToList();


            List<Comment> AddList = new List<Comment>();
            foreach (var item in departmentCommunicationTypes)
            {
                var GetComments = dbContext.Comments.Include(x => x.Communication).Where(x => x.Communication.CommunicationTypeId == item.Id && x.CommunicationId == Conversation.Id).ToList().OrderByDescending(x => x.Id).FirstOrDefault();
                if (GetComments == null)
                {
                    GetComments = dbContext.Comments.Include(x => x.Communication).FirstOrDefault();
                }
                AddList.Add(GetComments);

            }

            return AddList;
        }


        public List<Comment> RefundFindDepartmentWithComments(int RCSApplicationId)
        {
            //GetAll CommunicationTypes
            var department = new CommunicationType();
            //var departmentCommunicationTypes = department.GetAllCommunicationActive();
            var departmentCommunicationTypes = dbContext.CommunicationTypes.Where(o => o.IsActive && !o.IsDeleted).ToList();

            //Get the current conversation
            var CommuniicationBuilder = dbContext.Communications.ToList();
            var Conversation = CommuniicationBuilder.Where(x => x.RefundApplicationId == RCSApplicationId).FirstOrDefault();

            //Get a list of comments for that conversation
            //var GetComments = dbContext.Comments.Where(x => x.CommunicationId == Conversation.Id).Include(c => c.Communication).Include(c => c.CreatedBySystemUser).Include(c => c.ModifiedBySystemUser).Include(c => c.ReferenceType).Include(c => c.SenderCustomer).Include(c => c.Communication).Include(c => c.CreatedBySystemUser).Include(c => c.ModifiedBySystemUser).Include(c => c.ReferenceType).Include(c => c.SenderCustomer).ToList();


            List<Comment> AddList = new List<Comment>();
            foreach (var item in departmentCommunicationTypes)
            {
                var GetComments = dbContext.Comments.Include(x => x.Communication).Where(x => x.Communication.CommunicationTypeId == item.Id && x.CommunicationId == Conversation.Id).ToList().OrderByDescending(x => x.Id).FirstOrDefault();
                if (GetComments == null)
                {
                    GetComments = dbContext.Comments.Include(x => x.Communication).FirstOrDefault();
                }
                AddList.Add(GetComments);

            }

            return AddList;
        }
        #endregion
    }
}