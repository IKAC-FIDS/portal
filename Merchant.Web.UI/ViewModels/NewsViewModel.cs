using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace TES.Merchant.Web.UI.ViewModels
{
    public class StorageLogDto
    {
        public string Operation { get; set; }
        public string User { get; set; }
        public string Date { get; set; }
        public int Value { get; set; }
        public int StorageId { get; set; }
        public string StorageTitle { get; set; }
    }
    public class NewsViewModel
    {
        public long Id { get; set; }
        public string Title { get; set; }

        [AllowHtml]
        public string Body { get; set; }

        public bool IsMain { get; set; }

        public DateTime PublishDate { get; set; }
        
        public List<HttpPostedFileBase> PostedFiles { get; set; }
        public List<AttachFile> AttachFiles { get; set; }
    }

    public class AttachFile
    {
        public  int Id { get; set; }
        public  string Title { get; set; }
    }
    public class SecondSubjectListViewModel
    {
        public  int ParentId { get; set; }
        public  List<NewsViewModel> SecondSubject { get; set; }
        public string Title { get; set; }
        public  string ParentTitle { get; set; }
    }
    
    

    public class MessageSubjectViewModel
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public  int PspId { get; set; }
        
    }
    
    public class PeopleRatingViewModel
    {
        public long Id { get; set; }
        public string FullName { get; set; }
        public  string Branch { get; set; }
        public int  Rating { get; set; }

       
    }
}