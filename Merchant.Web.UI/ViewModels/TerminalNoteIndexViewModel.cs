using System;
using System.Collections.Generic;

namespace TES.Merchant.Web.UI.ViewModels
{
    public class TerminalNoteIndexViewModel
    {
        public long TerminalId { get; set; }

        public List<TerminalNoteIndexViewModel.TerminalNoteViewModel> Notes { get; set; }

        public class TerminalNoteViewModel
        {
            public int Id { get; set; }
            public string Body { get; set; }
            public long TerminalId { get; set; }
            public long SubmitterUserId { get; set; }
            public DateTime SubmitTime { get; set; }
            public string OrganizationUnitTitle { get; set; }
            public long? OrganizationUnitId { get; set; }
            public string SubmitterUserFullName { get; set; }
        }
    }
}