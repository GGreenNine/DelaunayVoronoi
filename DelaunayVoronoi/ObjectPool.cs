using System.Collections.Generic;
using System;
using System.Collections.Concurrent;
using System.Windows;
using System.Windows.Shapes;
using HappyUnity.Spawners.ObjectPools;

namespace HappyUnity.Spawners.ObjectPools
{
    public class ObjectPool<T> where T : Shape
    {
        private List<T> _objects;
        private Func<T> _objectGenerator;

        private List<T> parkageStorage;

        public ObjectPool(Func<T> objectGenerator, int capacity = 10)
        {
            if (objectGenerator == null) throw new ArgumentNullException("objectGenerator");

            _objects         = new List<T>(capacity);
            parkageStorage   = new List<T>(capacity);
            _objectGenerator = objectGenerator;
        }

        public void GetObject(out T item)
        {
            var lastIndex = _objects.Count - 1;
            item = _objects[lastIndex];
            _objects.RemoveAt(lastIndex);

            if (item != null)
            {
                item.Visibility = Visibility.Visible;
            }
            else
            {
                item = _objectGenerator();
            }
            parkageStorage.Add(item);
        }

        public void PutObject(T item)
        {
            item.Visibility = Visibility.Hidden;
            _objects.Add(item);
        }

        public void ClearParkage()
        {
            foreach (var shape in parkageStorage)
            {
                PutObject(shape);
            }
            parkageStorage = new List<T>();
        }
    }
}