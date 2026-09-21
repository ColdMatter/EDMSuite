"""Makes `import ybf` work when running pytest from the repository root,
even if the package has not been pip-installed yet."""

import sys
from pathlib import Path

ROOT = Path(__file__).parent.resolve()
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))
