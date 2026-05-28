import pyvisa
import time
import csv
import numpy as np

# File name
FileName = r"C:\Users\UEDM\Imperial College London\Team ultracold - PH - Documents\Data\2026\2026-05\20260505\BiasCurrentTrace\Twinleaf_2mA_0.02NPLC.csv"

# Create resource manager
rm = pyvisa.ResourceManager()

# Open USB connection (update with your actual VISA address)
inst = rm.open_resource("GPIB0::12::INSTR")

# Increase timeout for bulk transfer
inst.timeout = 20000  # ms

# Identify instrument
print(inst.query("*IDN?"))

# Reset and clear
inst.write("*RST")
inst.write("*CLS")

# Configure for fastest DC current measurement
inst.write("CONF:CURR:DC")
inst.write("CURR:DC:RANG:AUTO ON")

# Set fastest integration time (lowest accuracy, highest speed), 0.02 means 400us
inst.write("CURR:DC:NPLC 0.02")

# Disable auto-zero for speed
inst.write("ZERO:AUTO OFF")

# Trigger immediately for all samples
inst.write("TRIG:SOUR IMM")
inst.write("INIT")

NrSamples = 100
Readings = np.full(NrSamples,np.nan)
TimesStart = np.full(NrSamples,np.nan)
TimesEnd = np.full(NrSamples,np.nan)
for ind in range(NrSamples):
    # Start measurement
    tStart = time.time_ns()
    TimesStart[ind] = tStart
    
    # Fetch all readings at once
    data = inst.query("READ?")
    tEnd = time.time_ns()
    Readings[ind] = float(data.split(',')[0])
    TimesEnd[ind] = tEnd
    # Convert to list of floats

# Close connection
inst.close()

# Summary
print(f"Collected {len(Readings)} readings")
print(f"Time elapsed: {(tEnd-TimesStart[0])/1e9}")
print(f"First 5: {Readings[:5]}")

# Write to file
with open(FileName, "w", newline='') as file:
    writer = csv.writer(file)
    writer.writerow(['TimeStart', 'TimeEnd', 'Current (A)'])

    for i in range(len(Readings)):
        current_value = float(Readings[i])
        
        # Write to CSV
        writer.writerow([TimesStart[i],TimesEnd[i], current_value])
