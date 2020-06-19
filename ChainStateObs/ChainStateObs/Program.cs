using System;
using System.Collections.Generic;

namespace ChainStateObs
{
    class Program
    {
        static void Main(string[] args)
        {

            Request r = new Request(RequestType.Plovdiv);
            Boy boy = new Boy();


            PayDesk foreignPayDesk = new ForeignPayDesk("Foreign Pay Desk");
            PayDesk countryPayDesk = new CountryPayDesk("Country Pay Desk");
            PayDesk plovdivPayDesk = new PlovdivPayDesk("Plovdiv Pay Desk");

            foreignPayDesk.AddObserver(boy);
            countryPayDesk.AddObserver(boy);
            plovdivPayDesk.AddObserver(boy);


            foreignPayDesk.Successor = plovdivPayDesk;
            plovdivPayDesk.Successor = countryPayDesk;

            foreignPayDesk.Handle(r);

            Console.Read();
        }
    }
    ////////////////////////////////////////////////////////////////////////////////
    class Request
    {
        public RequestType RequestType { get; set; }
        public Request(RequestType rt)
        {
            RequestType = rt;
        }
    }
    ////////////////////////////////////////////////////////////////////////////////
    abstract class PayDesk : IObservable
    {
        public string DeskName { get; set; }
        public IPayDeskState State { get; set; }
        public PayDesk Successor { get; set; }
        protected List<IObserver> Observers { get; set; } = new List<IObserver>();

        public void AddObserver(IObserver o)
        {
            Observers.Add(o);
        }

        public void RemoveObserver(IObserver o)
        {
            Observers.Remove(o);
        }

        public void NotifyObservers()
        {
            foreach (IObserver observer in Observers)
                observer.Update(this);
        }

        public abstract void Handle(Request request);

    }
    ////////////////////////////////////////////////////////////////////////////////
    class ForeignPayDesk : PayDesk
    {
        public ForeignPayDesk(string deskname)
        {
            DeskName = deskname;
            State = new StayAndWaitDeskState();
            State.StayAndWait(this);
        }
        public override void Handle(Request request)
        {
            if (request.RequestType == RequestType.Foreign)
            {
                State = new PrepareDeskState();
                State.Prepare(this);

                NotifyObservers();
            }
            else if (Successor != null)
                Successor.Handle(request);
        }
    }
    ////////////////////////////////////////////////////////////////////////////////
    class CountryPayDesk : PayDesk
    {
        public CountryPayDesk(string deskname)
        {
            DeskName = deskname;
            State = new StayAndWaitDeskState();
            State.StayAndWait(this);
        }
        public override void Handle(Request request)
        {
            if (request.RequestType == RequestType.Country)
            {
                State = new PrepareDeskState();
                State.Prepare(this);

                NotifyObservers();
            }
            else if (Successor != null)
                Successor.Handle(request);
        }
    }
    ////////////////////////////////////////////////////////////////////////////////
    class PlovdivPayDesk : PayDesk
    {
        public PlovdivPayDesk(string deskname)
        {
            DeskName = deskname;
            State = new StayAndWaitDeskState();
            State.StayAndWait(this);
        }
        public override void Handle(Request request)
        {
            if (request.RequestType == RequestType.Plovdiv)
            {
                State = new PrepareDeskState();
                State.Prepare(this);

                NotifyObservers();
            }
            else if (Successor != null)
                Successor.Handle(request);
        }
    }
    ////////////////////////////////////////////////////////////////////////////////
    class Boy : IObserver
    {
        public void Update(PayDesk payDesk)
        {
            Console.WriteLine("Boy took the parcel");
            payDesk.State = new StayAndWaitDeskState();
            payDesk.State.StayAndWait(payDesk);
        }
    }
    ////////////////////////////////////////////////////////////////////////////////
    enum RequestType
    {
        Foreign, Country, Plovdiv
    }
    ////////////////////////////////////////////////////////////////////////////////
    interface IObservable
    {
        void AddObserver(IObserver o);
        void RemoveObserver(IObserver o);
        void NotifyObservers();
    }
    ////////////////////////////////////////////////////////////////////////////////
    interface IObserver
    {
        void Update(PayDesk payDesk);
    }
    ////////////////////////////////////////////////////////////////////////////////
    interface IPayDeskState
    {
        void StayAndWait(PayDesk payDesk);
        void Prepare(PayDesk payDesk);
    }
    ////////////////////////////////////////////////////////////////////////////////
    class StayAndWaitDeskState : IPayDeskState
    {
        public void StayAndWait(PayDesk payDesk)
        {
            Console.WriteLine(payDesk.DeskName + " Continue Staying and waiting");
        }

        public void Prepare(PayDesk payDesk)
        {
            Console.WriteLine(payDesk.DeskName + " Preparing parcel");
            payDesk.State = new PrepareDeskState();
        }
    }
    ////////////////////////////////////////////////////////////////////////////////
    class PrepareDeskState : IPayDeskState
    {
        public void Prepare(PayDesk payDesk)
        {
            Console.WriteLine(payDesk.DeskName + " Preparing parcel");
        }

        public void StayAndWait(PayDesk payDesk)
        {
            Console.WriteLine(payDesk.DeskName + " Staying and waiting");
            payDesk.State = new PrepareDeskState();
        }
    }
}
