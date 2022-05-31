using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ObjectiveBlank : Objective
{

    //This requires checking of total-function, basically a predicate. We could check it every frame.

    public ObjectiveBlank(ConfigObjective.Objective _objective)
    {
        objective_config = _objective;
    }
}

