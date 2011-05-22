﻿namespace MvcTurbine.Web {
    using System;
    using System.Collections.Generic;
    using System.Web;
    using ComponentModel;
    using Properties;
    using Config;

    /// <summary>
    /// Class that provides the simple IoC support for ASP.NET MVC.
    /// </summary>
    public class TurbineApplication : HttpApplication, ITurbineApplication {
        private static IRotorContext rotorContext;
        private static IServiceLocator serviceLocator;

        /// <summary>
        /// Gets or sets the current implementation of <see cref="IServiceLocator"/>
        /// the application instance will use.
        /// </summary>
        public IServiceLocator ServiceLocator {
            get { return serviceLocator; }
            set { serviceLocator = value; }
        }

        /// <summary>
        /// Gets or sets the current <see cref="RotorContext"/> for the application instance to use.
        /// </summary>
        public IRotorContext CurrentContext {
            get { return rotorContext; }
            set { rotorContext = value; }
        }

        /// <summary>
        /// Turns the current context
        /// </summary>
        public void TurnRotor() {
            CurrentContext = GetContext();
            ExecuteContext();
        }

        /// <summary>
        /// Sets up the engine with the specified pieces.
        /// </summary>
        public virtual void SetupEngine() {
            Engine
				.Initialize
                .ConfigureWithServiceLocator(ServiceLocator);
        }

        /// <summary>
        /// Performs any startup processing.
        /// </summary>
        public virtual void Startup() {
        }

        /// <summary>
        /// Shuts down the current application.
        /// </summary>
        public virtual void Shutdown() {
        }

        /// <summary>
        /// Sets up one-time only execution for the application.
        /// </summary>
        protected void Application_Start(object sender, EventArgs e) {
            Startup();

            ServiceLocator = GetServiceLocator();

            PostServiceLocatorAcquisition();

            SetupEngine();

            TurnRotor();
        }

        /// <summary>
        /// Executed after the <see cref="IServiceLocator"/> has been acquired.
        /// </summary>
        protected virtual void PostServiceLocatorAcquisition() {
        }

        /// <summary>
        /// Initializes and execute the current <see cref="RotorContext"/>.
        /// </summary>
        protected virtual void ExecuteContext() {
            CurrentContext.Turn();
        }

        /// <summary>
        /// Initializes the current <see cref="HttpApplication"/>.
        /// </summary>
        public override void Init() {
            base.Init();

            InitializeHttpModules();

            if (CurrentContext == null) return;
            CurrentContext.Initialize(this);
        }

        /// <summary>
        /// Initializes all the registered <see cref="IHttpModule"/> instances.
        /// </summary>
        /// <remarks>
        /// This code has to live here in order for the pieces to work correctly with
        /// the ASP.NET runtime on IIS6/7.
        /// </remarks>
        protected virtual void InitializeHttpModules() {
            var modules = ServiceLocator.ResolveServices<IHttpModule>();
            if (modules == null || modules.Count == 0) return;

            foreach (var module in modules) {
                module.Init(this);
            }
        }

        /// <summary>
        /// Tears down, one-time only, the application.
        /// </summary>
        protected virtual void Application_End() {
            Shutdown();

            ShutdownContext();
        }

        /// <summary>
        /// Shuts down the <see cref="CurrentContext"/> and handles all pieces of cleanup.
        /// </summary>
        protected virtual void ShutdownContext() {
            if (CurrentContext == null) return;
            
            CurrentContext.Dispose();

            CurrentContext = null;
            ServiceLocator = null;
        }

        /// <summary>
        /// Gets the instance of <see cref="RotorContext"/> that is registered with the 
        /// <see cref="IServiceLocator"/>. 
        /// </summary>
        /// <returns>The registered <see cref="RotorContext"/>, otherwise a default <see cref="RotorContext"/> is used.</returns>
        protected virtual IRotorContext GetContext() {
            try {
                return ServiceLocator.Resolve<IRotorContext>();
            } catch {
                return new RotorContext(ServiceLocator);
            }
        }

        /// <summary>
        /// Gets the instance of <see cref="IServiceLocator"/> that is registered with
        /// <see cref="ServiceLocatorManager.SetLocatorProvider"/>.
        /// </summary>
        protected virtual IServiceLocator GetServiceLocator() {
            var locator = ServiceLocatorManager.Current;
            
            if (locator == null) {
                throw new InvalidOperationException(Resources.ServiceLocatorExceptionMessage);
            }

            return locator;
        }
    }
}
