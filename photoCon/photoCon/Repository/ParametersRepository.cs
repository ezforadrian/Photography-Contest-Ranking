using Microsoft.EntityFrameworkCore;
using photoCon.Data;
using photoCon.Interface;
using photoCon.Models;

namespace photoCon.Repository
{
    public class ParametersRepository : IParametersRepository
    {
        private readonly TallyProgramContext _tallyProgramContext;

        public ParametersRepository(TallyProgramContext tallyProgramContext)
        {
            _tallyProgramContext = tallyProgramContext;
        }

        public async Task<TblParameter> ParamDate(string parameterDescription)
        {
            return await _tallyProgramContext.TblParameters.Where(a => a.ParamDescription == parameterDescription).FirstOrDefaultAsync();
        }


        public async Task<ICollection<TblParameter>> GetTblParametersAsync()
        {
            return await _tallyProgramContext.TblParameters.ToListAsync();
        }
    }
}
