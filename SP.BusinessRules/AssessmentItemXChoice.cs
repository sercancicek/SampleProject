using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.BusinessRules
{
    public class AssessmentItemXChoice
    {
        public int Order { get; set; }
        public string Value { get; set; }
        public int ScoreValue { get; set; }
        public int AssessmentItem_ID { get; set; }
        public int ParentID { get; set; }
        public bool IsDefault { get; set; }
        public int ChoiceID { get; set; }
    }
}
