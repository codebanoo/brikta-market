﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VM.PVM.Base;

namespace VM.PVM.Teniaco
{
    public class GetAllEvaluationQuestionsListPVM : BPVM
    {
        public int? EvaluationCategoryId { get; set; }

        public string EvaluationQuestionSearch { get; set; }
    }
}
