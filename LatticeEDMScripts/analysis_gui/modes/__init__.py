"""The analysis tabs, in the order they appear"""

from analysis_gui.modes.duration import DurationTab
from analysis_gui.modes.setpoint import SetpointTab
from analysis_gui.modes.slowing import SlowingTab
from analysis_gui.modes.tofcollector import TOFCollectorTab

TABS = [DurationTab, SetpointTab, SlowingTab, TOFCollectorTab]
