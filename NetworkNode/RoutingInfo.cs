using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkNode
{

    // klasa pewnie do ogarniecia gdy zrobimy Routing Controller
    public class RoutingInfo
    {
        public RoutingInfo()
        {
            routerLabels = new List<RouterLabel>();
            routerDeletedLabels = new List<RouterDeletedLabel>();
            routerActions = new List<RouterAction>();
        }
    }

    public class RouterLabel
    {
        public int inputPort;
        public int label;
        public int action;
        public int labelsStackId;

        public RouterLabel(int inputPort, int label, int action, int labelsStackId)
        {
            this.inputPort = inputPort;
            this.label = label;
            this.action = action;
            this.labelsStackId = labelsStackId;
        }
    }

}
