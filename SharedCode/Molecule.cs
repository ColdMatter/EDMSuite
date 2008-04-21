using System;
using System.Collections.Generic;
using System.Text;

namespace DecelerationConfig
{
    public class Molecule
    {
        private string name; //the name of the molecule
        private double mass; //in amu
        private double b; //in Hz
        private double dipole; //in Hz/(V/m)

        public Molecule()
        {
        }

        public Molecule(string name, double mass, double rotationalConstant, double dipole)
        {
            this.Name = name;
            this.Mass = mass;
            this.B = rotationalConstant;
            this.Dipole = dipole;
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public double Mass
        {
            get { return mass; }
            set { mass = value; }
        }

        public double B
        {
            get { return b; }
            set { b = value; }
        }

        public double Dipole
        {
            get { return dipole; }
            set { dipole = value; }
        }

    }
}
  