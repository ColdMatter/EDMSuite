import pyvisa
import time
import csv

# File name
FileName = r"C:\Users\UEDM\Imperial College London\Team ultracold - PH - Documents\Data\2026\2026-04\20260424\BiasCurrentTrace\Twinleaf_2mA_0p2NPLC.csv"

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
inst.write("CURR:DC:NPLC 0.2")

# Disable auto-zero for speed
inst.write("ZERO:AUTO OFF")

# Set number of samples
inst.write("SAMP:COUNT 500")

# Trigger immediately for all samples
inst.write("TRIG:SOUR IMM")
inst.write("TRIG:COUNT 1")

# Start measurement
tStart = time.time_ns()
inst.write("INIT")

# Fetch all readings at once
data = inst.query("FETCH?")
tEnd = time.time_ns()

# Convert to list of floats
readings = [float(x) for x in data.split(',')]

# Close connection
inst.close()

# Summary
print(f"Collected {len(readings)} readings")
print(f"Time elapsed: {(tEnd-tStart)/1e9}")
print(f"First 5: {readings[:5]}")

# Write to file
with open(FileName, "w", newline='') as file:
    writer = csv.writer(file)
    writer.writerow(['Reading', 'Current (A)'])

    for i in range(len(readings)):
        current_value = float(readings[i])
        
        # Write to CSV
        writer.writerow([i + 1, current_value])
