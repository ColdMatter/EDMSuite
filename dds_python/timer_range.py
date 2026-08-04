"""Find the true accepted range of SPC_DDS_TRG_TIMER by bisection.

The manual quotes 83.2 ns .. 27.48 s with 6.4 ns resolution. Out-of-range
values are rejected with err 257 ("value not allowed") rather than clamped, so
callers that ignore return codes silently keep the previous setting.

    poetry run python dds_python/timer_range.py
"""

from spectrum_dds import DDSCard, SpcmError


def main():
    with DDSCard() as card:
        card.configure_dds()

        def accepted(t):
            try:
                card.set_trg_timer(t)
                return True
            except SpcmError:
                return False

        lo, hi = 1.0, 100.0
        assert accepted(lo), "1 s should be accepted"
        while not accepted(lo):
            lo /= 2
        for _ in range(50):
            mid = (lo + hi) / 2
            if accepted(mid):
                lo = mid
            else:
                hi = mid
        print("max accepted : %.9f s" % lo)
        print("min rejected : %.9f s" % hi)

        lo, hi = 1e-12, 1e-6
        for _ in range(60):
            mid = (lo + hi) / 2
            if accepted(mid):
                hi = mid
            else:
                lo = mid
        print("min accepted : %.12g s  (reads back %.12g s)"
              % (hi, card.set_trg_timer(hi)))
        print("max rejected : %.12g s" % lo)

        print("\nresolution check near 1 ms:")
        prev = None
        for n in range(156250, 156256):
            got = card.set_trg_timer(n * 6.4e-9)
            if prev is not None:
                print("  step %.4g s" % (got - prev))
            prev = got


if __name__ == "__main__":
    main()
