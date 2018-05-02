﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Enterwell.Clients.Wpf.Notifications.Controls
{
    /// <summary>
    /// The notification message container.
    /// </summary>
    /// <seealso cref="ItemsControl" />
    public class NotificationMessageContainer : ItemsControl
    {
        /// <summary>
        /// Gets or sets the manager.
        /// </summary>
        /// <value>
        /// The manager.
        /// </value>
        public INotificationMessageManager Manager
        {
            get { return (INotificationMessageManager)GetValue(ManagerProperty); }
            set { SetValue(ManagerProperty, value); }
        }

        /// <summary>
        /// The manager property.
        /// </summary>
        public static readonly DependencyProperty ManagerProperty =
            DependencyProperty.Register("Manager", typeof(INotificationMessageManager), typeof(NotificationMessageContainer), new PropertyMetadata(null, ManagerPropertyChangedCallback));

        /// <summary>
        /// Managers the property changed callback.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="dependencyPropertyChangedEventArgs">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        /// <exception cref="NullReferenceException">Dependency object is not of valid type " + nameof(NotificationMessageContainer)</exception>
        private static void ManagerPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var @this = dependencyObject as NotificationMessageContainer;
            if (@this == null)
                throw new NullReferenceException("Dependency object is not of valid type " + nameof(NotificationMessageContainer));
            
            if (dependencyPropertyChangedEventArgs.OldValue is INotificationMessageManager oldManager)
                @this.DetachManagerEvents(oldManager);
            
            if (dependencyPropertyChangedEventArgs.NewValue is INotificationMessageManager newManager)
                @this.AttachManagerEvents(newManager);
        }

        /// <summary>
        /// Attaches the manager events.
        /// </summary>
        /// <param name="newManager">The new manager.</param>
        private void AttachManagerEvents(INotificationMessageManager newManager)
        {
            newManager.OnMessageQueued += ManagerOnOnMessageQueued;
            newManager.OnMessageDismissed += ManagerOnOnMessageDismissed;
        }

        /// <summary>
        /// Detaches the manager events.
        /// </summary>
        /// <param name="oldManager">The old manager.</param>
        private void DetachManagerEvents(INotificationMessageManager oldManager)
        {
            oldManager.OnMessageQueued -= ManagerOnOnMessageQueued;
            oldManager.OnMessageDismissed -= ManagerOnOnMessageDismissed;
        }

        /// <summary>
        /// Manager on message dismissed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="NotificationMessageManagerEventArgs"/> instance containing the event data.</param>
        /// <exception cref="InvalidOperationException">Can't use both ItemsSource and Items collection at the same time.</exception>
        private void ManagerOnOnMessageDismissed(object sender, NotificationMessageManagerEventArgs args)
        {
            if (this.ItemsSource != null)
                throw new InvalidOperationException(
                    "Can't use both ItemsSource and Items collection at the same time.");

            if (args.Message is INotificationMessageAnimation)
            {
                var animatableMessage = args.Message as INotificationMessageAnimation;
                if (animatableMessage.Animates && animatableMessage.AnimatableElement != null)
                {
                    var animation = new DoubleAnimation
                    {
                        To = 0,
                        BeginTime = TimeSpan.FromSeconds(0),
                        Duration = TimeSpan.FromSeconds(animatableMessage.AnimationDuration),
                        FillBehavior = FillBehavior.Stop
                    };
                    animation.Completed += (s, a) => RemoveMessage(args.Message);

                    animatableMessage.AnimatableElement.BeginAnimation(UIElement.OpacityProperty, animation);
                }
                else
                {
                    RemoveMessage(args.Message);
                }
            }
            else
            {
                RemoveMessage(args.Message);
            }
        }

        private void RemoveMessage(INotificationMessage message)
        {
            this.Items?.Remove(message);
        }

        /// <summary>
        /// Manager on message queued.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="NotificationMessageManagerEventArgs"/> instance containing the event data.</param>
        /// <exception cref="InvalidOperationException">Can't use both ItemsSource and Items collection at the same time.</exception>
        private void ManagerOnOnMessageQueued(object sender, NotificationMessageManagerEventArgs args)
        {
            if (this.ItemsSource != null)
                throw new InvalidOperationException(
                    "Can't use both ItemsSource and Items collection at the same time.");

            this.Items?.Add(args.Message);

            if (args.Message is INotificationMessageAnimation)
            {
                var animatableMessage = args.Message as INotificationMessageAnimation;
                if (animatableMessage.Animates && animatableMessage.AnimatableElement != null)
                {

                    animatableMessage.AnimatableElement.Opacity = 0;
                    var animation = new DoubleAnimation
                    {
                        To = 1,
                        BeginTime = TimeSpan.FromSeconds(0),
                        Duration = TimeSpan.FromSeconds(animatableMessage.AnimationDuration),
                        FillBehavior = FillBehavior.Stop
                    };
                    animation.Completed += (s, a) => animatableMessage.AnimatableElement.Opacity = 1;

                    animatableMessage.AnimatableElement.BeginAnimation(UIElement.OpacityProperty, animation);
                }
            }
        }

        /// <summary>
        /// Initializes the <see cref="NotificationMessageContainer"/> class.
        /// </summary>
        static NotificationMessageContainer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NotificationMessageContainer), new FrameworkPropertyMetadata(typeof(NotificationMessageContainer)));
        }
    }
}
