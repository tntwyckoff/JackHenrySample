using m = JackHenry.Demo.Libraries.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JackHenry.Demo.Services.Statistics.StatIntegration
{
    public interface IStatSummaryIntegrator
    {

        m.TwitterStatSummary IntegrateSummaryData(m.TwitterStatSummary data1, m.TwitterStatSummary data2);

    }
}
