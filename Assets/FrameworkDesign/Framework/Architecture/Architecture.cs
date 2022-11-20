using System;
using System.Collections.Generic;

namespace FrameworkDesign
{
    public interface IArchitecture
    {
        /// <summary>
        /// ע��ϵͳ
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="system"></param>
        void RegisterSystem<T>(T system) where T : ISystem;

        /// <summary>
        /// ע�� Model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        void RegisterModel<T>(T model) where T : IModel;

        /// <summary>
        /// ע�� Utility
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="utility"></param>
        void RegisterUtility<T>(T utility) where T : IUtility;

        /// <summary>
        /// ��ȡ System
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetSystem<T>() where T : class, ISystem;

        /// <summary>
        /// ��ȡ Model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetModel<T>() where T : class, IModel;

        /// <summary>
        /// ��ȡ����
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetUtility<T>() where T : class, IUtility;

        /// <summary>
        /// ��������
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void SendCommand<T>() where T : ICommand, new();

        /// <summary>
        /// ��������
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        void SendCommand<T>(T command) where T : ICommand;

        /// <summary>
        /// ���Ͳ�ѯ
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        TResult SendQuery<TResult>(IQuery<TResult> query);

        /// <summary>
        /// �����¼�
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void SendEvent<T>() where T : new();
        void SendEvent<T>(T e);

        /// <summary>
        /// ע���¼�
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="onEvent"></param>
        /// <returns></returns>
        IUnRegister RegisterEvent<T>(Action<T> onEvent);

        /// <summary>
        /// ע���¼�
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="onEvent"></param>
        void UnRegisterEvent<T>(Action<T> onEvent);
    }

    /// <summary>
    /// �ܹ�
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Architecture<T> : IArchitecture where T : Architecture<T>, new()
    {
        /// <summary>
        /// �Ƿ��ʼ�����
        /// </summary>
        private bool mInited = false;

        /// <summary>
        /// ���ڳ�ʼ���� Systems �Ļ���
        /// </summary>
        private List<ISystem> mSystems = new List<ISystem>();

        /// <summary>
        /// ���ڳ�ʼ���� Models �Ļ���
        /// </summary>
        private List<IModel> mModels = new List<IModel>();

        /// <summary>
        /// ����ע��
        /// </summary>
        public static Action<T> OnRegisterPath = architecture => { };

        private static T mArchitecture = null;

        public static IArchitecture Instance
        {
            get
            {
                if (mArchitecture == null)
                {
                    MakeSureArchitecture();
                }

                return mArchitecture;
            }
        }

        //ȷ��Container����ʵ��
        static void MakeSureArchitecture()
        {
            if (mArchitecture == null)
            {
                mArchitecture = new T();
                mArchitecture.Init();

                //����
                OnRegisterPath?.Invoke(mArchitecture);

                //��ʼ�� Model
                foreach (var architectureModel in mArchitecture.mModels)
                {
                    architectureModel.Init();
                }

                //��� Model 
                mArchitecture.mModels.Clear();

                //��ʼ�� System
                foreach (var architectureSystem in mArchitecture.mSystems)
                {
                    architectureSystem.Init();
                }

                //��� System
                mArchitecture.mSystems.Clear();
             
                mArchitecture.mInited = true;
            }
        }

        private IOCContainer mContainer = new IOCContainer();

        //��������ע��ģ��
        protected abstract void Init();

        ////�ṩһ����ȡģ��� API
        //public static T Get<T>() where T : class
        //{
        //    MakeSureArchitecture();
        //    return mArchitecture.mContainer.Get<T>();
        //}

        ////�ṩһ��ע��ģ��� API
        //public static void Register<T>(T instance)
        //{
        //    MakeSureArchitecture();
        //    mArchitecture.mContainer.Register<T>(instance);
        //}

        public void RegisterSystem<TSystem>(TSystem system) where TSystem : ISystem
        {
            system.SetArchitecture(this);
            mContainer.Register<TSystem>(system);

            if (!mInited)
            {
                mSystems.Add(system);
            }
            else
            {
                system.Init();
            }
        }

        public void RegisterModel<TModel>(TModel model) where TModel : IModel
        {
            //��Ҫ�� Model ��ֵһ��
            model.SetArchitecture(this);
            mContainer.Register<TModel>(model);

            if (!mInited)
            {
                mModels.Add(model);
            }
            else
            {
                model.Init();
            }
        }

        public void RegisterUtility<TUtility>(TUtility utility) where TUtility : IUtility
        {
            mContainer.Register<TUtility>(utility);
        }

        TSystem IArchitecture.GetSystem<TSystem>()
        {
            return mContainer.Get<TSystem>();
        }

        TModel IArchitecture.GetModel<TModel>()
        {
            return mContainer.Get<TModel>();
        }

        public TUtility GetUtility<TUtility>() where TUtility : class, IUtility
        {
            return mContainer.Get<TUtility>();
        }

        public void SendCommand<TCommand>() where TCommand : ICommand, new()
        {
            var command = new TCommand();
            command.SetArchitecture(this);
            command.Execute();
        }

        public void SendCommand<TCommand>(TCommand command) where TCommand : ICommand
        {
            command.SetArchitecture(this);
            command.Execute();
        }

        public TResult SendQuery<TResult>(IQuery<TResult> query)
        {
            query.SetArchitecture(this);
            return query.Do();
        }

        private ITypeEventSystem mTypeEventSystem = new TypeEventSystem();

        public void SendEvent<TEvent>() where TEvent : new()
        {
            mTypeEventSystem.Send<TEvent>();
        }

        public void SendEvent<TEvent>(TEvent e)
        {
            mTypeEventSystem.Send<TEvent>(e);
        }

        public IUnRegister RegisterEvent<TEvent>(Action<TEvent> onEvent)
        {
            return mTypeEventSystem.Register<TEvent>(onEvent);
        }

        public void UnRegisterEvent<TEvent>(Action<TEvent> onEvent)
        {
            mTypeEventSystem.UnRegister<TEvent>(onEvent);
        }       
    }
}

