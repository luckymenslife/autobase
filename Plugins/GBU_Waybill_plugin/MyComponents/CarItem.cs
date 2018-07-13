using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GBU_Waybill_plugin
{
    public class CarItem
    {
        private string _carType;
        private int _carTypeId;
        private string _gosNum;
        private string _garNum;
        private string _regNum;
        private float _carNorm;
        private float _carEquipNorm;
        private float _car100kmNorm;
        private int _deptId;
        private bool _isComplex;
        private int _regime;
        private int _regimeHours;
        private int _service;
        private string _owner;
        private float _fuelTankCapacity;
        private bool _plRegime;

        public CarItem(string carType, int carTypeId, string gosNum, string garNum, string regNum, float carNorm, 
            float carEquipNorm, float car100kmNorm, int deptId, bool isComplex, int regime, int regimeHours, 
            int service, string owner, float fuelTankCapacity, bool plRegime)
        {
            _carType = carType;
            _carTypeId = carTypeId;
            _gosNum = gosNum;
            _garNum = garNum;
            _regNum = regNum;
            _carNorm = carNorm;
            _carEquipNorm = carEquipNorm;
            _car100kmNorm = car100kmNorm;
            _deptId = deptId;
            _isComplex = isComplex;
            _regime = regime;
            _regimeHours = regimeHours;
            _service = service;
            _owner = owner;
            _fuelTankCapacity = fuelTankCapacity;
            _plRegime = plRegime;
        }

        public String getCarType
        {
            get { return this._carType; }
        }

        public int getCarTypeId
        {
            get { return this._carTypeId; }
        }

        public String getGosNum
        {
            get { return this._gosNum; }
        }

        public String getGarNum
        {
            get { return this._garNum; }
        }

        public String getRegNum
        {
            get { return this._regNum; }
        }

        public float getCarNorm
        {
            get { return this._carNorm; }
        }

        public float getCarEquipNorm
        {
            get { return this._carEquipNorm; }
        }

        public float getCar100kmNorm
        {
            get { return this._car100kmNorm; }
        }

        public int getDeptId
        {
            get { return this._deptId; }
        }

        public bool getIsComplex
        {
            get { return this._isComplex; }
        }

        public int getRegime
        {
            get { return this._regime; }
        }

        public int getRegimeHours
        {
            get { return this._regimeHours; }
        }

        public int getService
        {
            get { return this._service; }
        }

        public String getOwner
        {
            get { return this._owner; }
        }

        public float getFuelTankCapacity
        {
            get { return this._fuelTankCapacity; }
        }

        public bool getMotoPlRegime
        {
            get { return this._plRegime; }
        }
    }
}
