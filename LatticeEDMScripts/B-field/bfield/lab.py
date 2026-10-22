"""Which lab are we in: the bench, or the simulator?

A Lab hands out device objects. Everything above this module asks the
Lab for a sensor, a DAQ, a degausser or a scope and never imports a
vendor package itself, so the same procedure runs on either.

    lab = open_lab(config)                       # the bench
    lab = open_lab(config, sim=True, speed=60)   # simulated, 60x fast

Simulated runs write under <B-field>/sim_data (or `sim_dir`), never
into the real data_root, so a rehearsal cannot pollute the record.
"""

from __future__ import annotations

import os

from bfield import clock
from bfield.config import PROJECT_DIR, load_config
from bfield.errors import ConfigError

SIM_DATA_DIR = os.path.join(PROJECT_DIR, "sim_data")


class Lab:
    def __init__(self, config, sim=False, speed=1.0, sim_dir=None):
        self.config = config
        self.sim = bool(sim)
        if not self.sim and speed != 1.0:
            raise ConfigError("the clock can only be sped up in the "
                              "simulated lab (--sim)")
        self.speed = float(speed)
        self.world = None
        if self.sim:
            from bfield.devices.simulated import World
            self.sim_dir = os.path.abspath(sim_dir or SIM_DATA_DIR)
            os.makedirs(self.sim_dir, exist_ok=True)
            from bfield.sensors import load_sensors
            self.world = World(os.path.join(self.sim_dir, "world.json"),
                               sensors=load_sensors(config))
        else:
            self.sim_dir = None
        clock.set_speed(self.speed)

    # ---------------------------------------------------------- paths

    @property
    def data_root(self):
        return self.sim_dir if self.sim else self.config.data_root

    def bucket(self, key):
        return os.path.join(self.data_root,
                            self.config["paths"][f"bucket_{key}"])

    @property
    def dated(self):
        """[paths] layout = "dated": a run with no --outdir goes to
        <data_root>/<Mon YYYY>/<DD>/<NNN>_<name>. Otherwise (the old
        layout) to the program's bucket."""
        return self.config["paths"].get("layout", "buckets") == "dated"

    def new_run_dir(self, name):
        """Create the next dated run folder (see bfield.io.files)."""
        from bfield.io.files import dated_run_dir
        return dated_run_dir(self.data_root, name)

    # -------------------------------------------------------- devices

    def qzfm_backend(self):
        if self.sim:
            from bfield.devices.simulated import qzfm_backend
            return qzfm_backend(self.world)
        from bfield.devices.qzfm import real_qzfm_backend
        return real_qzfm_backend

    def sensor(self, device_name, ctx, *, head="a", zero_cond=None,
               lock_timeout=None):
        from bfield.devices.qzfm import QZFMSensor
        d = self.config.section("qzfm").get("defaults") or {}
        return QZFMSensor(
            device_name, ctx, head=head,
            zero_cond=d.get("zero_cond_pT_per_s", 100.0)
            if zero_cond is None else zero_cond,
            lock_timeout=d.get("lock_timeout_s", 600.0)
            if lock_timeout is None else lock_timeout,
            backend=self.qzfm_backend())

    def daq(self):
        """A DAQ object (used inside the recorder child)."""
        if self.sim:
            from bfield.devices.simulated import SimUSB6008
            return SimUSB6008(self.world)
        from bfield.devices.daq import USB6008
        return USB6008()

    def degausser(self, ctx, warnings, port=None):
        from bfield.devices.degausser import TwinleafDG
        port = port or self.config.section("degausser").get("port")
        if self.sim:
            from bfield.devices.simulated import dg_backend
            return TwinleafDG(port, ctx, warnings,
                              backend=dg_backend(self.world))
        return TwinleafDG(port, ctx, warnings)

    def scope(self, ctx, resource=None, channel=None):
        from bfield.devices.scope import TBS1052B
        sc = self.config.section("scope")
        resource = resource or sc.get("resource") or None
        channel = channel or sc.get("channel") or "CH1"
        if self.sim:
            from bfield.devices.simulated_scope import FakeScopeBackend
            return TBS1052B(ctx, resource, channel,
                            backend=FakeScopeBackend(self.world))
        return TBS1052B(ctx, resource, channel)

    def plug(self, sensor_info):
        """The sensors of a run are plugged into their DAQ inputs. On the
        bench that is done by hand (the session says which goes where);
        in the simulator it rewires the world so the DAQ reads them."""
        if not self.sim:
            return
        wiring = {}
        for rec in sensor_info:
            for axis, pin in (rec.get("inputs") or {}).items():
                wiring[pin] = [rec["usb_serial"], axis]
        if wiring:
            self.world.update(lambda d: d.update(wiring=wiring))

    # ------------------------------------------------- child processes

    def job_block(self):
        """What a recorder child needs to rebuild this Lab."""
        return {"sim": self.sim, "speed": self.speed,
                "wall_at_spawn": clock.time(),
                "sim_dir": self.sim_dir,
                "config_sources": list(getattr(self.config, "sources", [])),
                "config": dict(self.config)}


def open_lab(config=None, *, sim=False, speed=1.0, sim_dir=None):
    return Lab(config or load_config(), sim=sim, speed=speed,
               sim_dir=sim_dir)


def lab_from_job(block):
    """Rebuild the parent's Lab inside the recorder child."""
    from bfield import heads
    from bfield.config import LabConfig
    cfg = LabConfig(block["config"])
    cfg.sources = block.get("config_sources", [])
    from bfield.sensors import aliases
    heads.set_aliases(aliases(cfg))
    lab = Lab(cfg, sim=block["sim"], speed=block["speed"],
              sim_dir=block.get("sim_dir"))
    if lab.sim and block.get("wall_at_spawn") is not None:
        clock.set_speed(lab.speed, wall=block["wall_at_spawn"])
    return lab
