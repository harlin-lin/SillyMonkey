﻿using DataInterface;
using FileReader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileReader {
    public class TestItems
    {
        private Dictionary<TestID, IItemInfo> _testItems;
        private Dictionary<TestID, int> _itemIndexes;

        public TestItems(int capacity) {
            _testItems = new Dictionary<TestID, IItemInfo>(capacity);
            _itemIndexes = new Dictionary<TestID, int>(capacity);
        }

        public int ItemsCount {
            get {
                return _itemIndexes.Count;
            }
        }

        public bool ExistTestItem(TestID testID) {
            if (_testItems.ContainsKey(testID))
                return true;
            else
                return false;
        }
        
        public int GetIndex(TestID testID) {
            int index;
            if (_itemIndexes.TryGetValue(testID, out index))
                return index;
            else
                throw new Exception("Do Check the testID if exist first!");
        }

        public bool AddTestItem(TestID testID, IItemInfo itemInfo) {
            //if (_testItems.ContainsKey(testID))
            //    return false;
            //else {
            _testItems.Add(testID, itemInfo);
            _itemIndexes.Add(testID, _testItems.Count - 1);
            //_ItemFilter.Add(true);
            //}
            return true;
        }

        public void UpdateTestText(TestID testID, string newTestText) {
            ((ItemInfo)_testItems[testID]).SetTestText(newTestText);
        }

        public List<TestID> GetTestIDsDefault() {
            return _testItems.Keys.ToList();
        }
        public IItemInfo GetItemInfo(TestID testID) {
            IItemInfo info;
            if (_testItems.TryGetValue(testID, out info))
                return info;
            else
                return null;
        }
        public Dictionary<TestID, IItemInfo> GetTestIDs_Info() {
            return _testItems;
        }
        public Dictionary<TestID, IItemInfo> GetTestIDs_InfoFiltered(bool[] itemsFilter) {
            Dictionary<TestID, IItemInfo> rst = new Dictionary<TestID, IItemInfo>(_testItems.Count);
            for(int i=0; i < _testItems.Count; i++) {
                if (!itemsFilter[i])
                    rst.Add(_testItems.ElementAt(i).Key, _testItems.ElementAt(i).Value);
            }

            return rst;
        }

        public List<TestID> GetTestIDsFiltered(bool[] itemsFilter) {
            List<TestID> testIDs = new List<TestID>(_testItems.Count);
            for (int i = 0; i < _testItems.Count; i++) {
                if (!itemsFilter[i])
                    testIDs.Add(_testItems.ElementAt(i).Key);
            }
            return testIDs;
        }

        public void UpdateItemFilter(FilterSetup filter, ref bool[] itemsFilter) {
            for (int i = 0; i < _testItems.Count; i++) {
                if (filter.maskTestIDs.Contains(_testItems.Keys.ElementAt(i)))
                    itemsFilter[i] = true;
            }
        }

    }
}
