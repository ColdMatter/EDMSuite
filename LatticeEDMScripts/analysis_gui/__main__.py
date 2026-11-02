"""python -m analysis_gui"""

from analysis_gui import env
env.setup()

from analysis_gui.app import main

if __name__ == "__main__":
    main()
