using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCIQUSTICKETS.COMMON.Enums
{
	public enum EmployeeRecipientMode
	{
		PreferenceBased = 0,   // current behavior — each employee's own toggle decides
		SelectedEmployees = 1  // only the configured list gets it, regardless of personal prefs
	}
}
