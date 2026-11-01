"""The DAQ card is a config choice: PCIe-6321 (now) or USB-6008 (before)."""

import pytest

from bfield import daq_models
from bfield.channels import parse_map
from bfield.config import load_config, with_overrides
from bfield.errors import ConfigError


def test_lab_config_is_the_pcie6321_on_dev3():
    cfg = load_config()
    m = daq_models.model_of(cfg)
    assert m.key == "pcie6321" and cfg["daq"]["device"] == "Dev3"
    assert cfg["daq"]["vrange_V"] in m.vranges


def test_models():
    new, old = daq_models.PCIE6321, daq_models.USB6008
    assert new.inputs == tuple(f"ai{i}" for i in range(8))
    assert old.inputs == ("ai0", "ai1", "ai2", "ai3")
    assert 2.0 not in new.vranges and 2.0 in old.vranges
    assert old.lsb(2.0) == 2.0 / 2048          # 12 bit, as before
    assert new.lsb(5.0) == 5.0 / 32768
    assert new.smallest_vrange(3.0) == 5.0 and old.smallest_vrange(1.0) == 2.0
    assert new.smallest_vrange(9.0) is None


def test_no_model_is_the_usb6008():
    # session jsons from before 2026-10-20 name no model
    assert daq_models.model_of({"daq": {"device": "Dev2"}}) is \
        daq_models.USB6008
    with pytest.raises(ConfigError, match="pxie6341"):
        daq_models.model_of({"daq": {"model": "pxie6341"}})


def test_map_inputs_follow_the_active_model():
    cfg = load_config()
    try:
        daq_models.set_active(with_overrides(cfg, {"daq": {"model":
                                                           "pcie6321"}}))
        assert parse_map("s1z:ai7")[0][2] == "ai7"
        daq_models.set_active(with_overrides(cfg, {"daq": {"model":
                                                           "usb6008"}}))
        with pytest.raises(ConfigError, match="USB-6008 has 4"):
            parse_map("s1z:ai7")
    finally:
        daq_models.set_active(cfg)
