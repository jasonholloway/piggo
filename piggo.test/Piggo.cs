using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using JsonLens.Test3;
using Shouldly;
using Xunit;

namespace JsonLens.Test3 {
    

    //is it right for functors to change between stages?
    //it surely is, as this is the only way we can embed multiple algebras
    //each functor is essentially a token to be interpreted
    //the sequencing of them is up to our layers of interpreters

    public interface Functor { }

    public interface Functor<F> : Functor {
        Functor<F, A> Lift<A>(A a);
        Functor<F, B> Map<A, B>(Functor<F, A> prev, Func<A, B> ab);
    }
    
    public interface Functor<F, A> : Functor<F> {
        Functor<F, B> Map<B>(Func<A, B> ab);
    }


    public interface Monad : Functor { }

    public interface Monad<F> : Monad, Functor<F> {
        Monad<F, B> Bind<A, B>(Monad<F, A> prev, Func<A, Monad<F, B>> afb);
    }

    public interface Monad<F, A> : Monad<F>, Functor<F, A> 
    {
        new Monad<F, B> Map<B>(Func<A, B> ab);
        Monad<F, B> Bind<B>(Func<A, Monad<F, B>> afb);
    }

    

    public class Seq : Monad<Seq> {
        public Functor<Seq, A> Lift<A>(A a) {
            return new Seq<A>(a);
        }

        public Functor<Seq, B> Map<A, B>(Functor<Seq, A> prev, Func<A, B> ab) {
            throw new NotImplementedException();
        }

        public Monad<Seq, B> Bind<A, B>(Monad<Seq, A> prev, Func<A, Monad<Seq, B>> afb) {
            throw new NotImplementedException();
        }
    }
    
    public class Seq<A> : Seq, Monad<Seq, A> 
    {
        readonly IEnumerable<A> _items;

        public Seq(IEnumerable<A> items) {
            _items = items;
        }
        
        public Seq(params A[] items)
            : this(items.AsEnumerable()) { }

        public Monad<Seq, B> Bind<B>(Func<A, Monad<Seq, B>> afb)
            => new Seq<B>(_items.SelectMany(i => ((Seq<B>)afb(i))._items));

        Functor<Seq, B> Functor<Seq, A>.Map<B>(Func<A, B> ab)
            => Bind(a => new Seq<B>(ab(a)));
        
        Monad<Seq, B> Monad<Seq, A>.Map<B>(Func<A, B> ab)
            => Bind(a => new Seq<B>(ab(a)));
    }
    
    
    
    

    

    
    public class Id : Functor<Id>
    {
        public Id() { }

        public Functor<Id, A> Lift<A>(A a) 
            => new Id<A>(a);

        public Functor<Id, B> Map<A, B>(Functor<Id, A> prev, Func<A, B> ab)
            => prev.Map(ab);
    }

    public class Id<A> : Id, Functor<Id, A> 
    {
        readonly A _val;

        public Id(A a) {
            _val = a;
        }
        
        public Functor<Id, B> Map<B>(Func<A, B> ab) 
            => new Id<B>(ab(_val));
    }



    public interface Free<F> : Monad<Free<F>>
        where F : Functor<F> { } 
    
    public interface Free<F, A> : Free<F>, Monad<Free<F>, A> 
        where F : Functor<F> {}
    
    
    public class FreeInst<F, A> : Free<F, A>
        where F : Functor<F> 
    {
        public Functor<Free<F>, A1> Lift<A1>(A1 a) {
            throw new NotImplementedException();
        }

        public Functor<Free<F>, B> Map<A1, B>(Functor<Free<F>, A1> prev, Func<A1, B> ab) {
            throw new NotImplementedException();
        }

        public Monad<Free<F>, B> Bind<A1, B>(Monad<Free<F>, A1> prev, Func<A1, Monad<Free<F>, B>> afb) {
            throw new NotImplementedException();
        }

        Functor<Free<F>, B> Functor<Free<F>, A>.Map<B>(Func<A, B> ab) {
            throw new NotImplementedException();
        }

        public Monad<Free<F>, B> Bind<B>(Func<A, Monad<Free<F>, B>> afb) {
            throw new NotImplementedException();
        }

        Monad<Free<F>, B> Monad<Free<F>, A>.Map<B>(Func<A, B> ab) {
            throw new NotImplementedException();
        }
    }
    
    

    

    public sealed class Point<F, A> : FreeInst<F, A>
        where F : Functor<F>
    {
        public Point(F fa) {
                
        }
    }

    public sealed class Bind<FA, A, FB, B> : FreeInst<FB, B>
        where FA : Functor
        where FB : Functor
    {
        public Bind(Free<FA, A> src, Func<A, Free<FB, B>> fn) {
            //...
        }
    }


    public class Async : Functor {

        public Async() { }

        public Functor<A> Lift<A>(A a)
            => default;
    }
    

    
    public abstract class CatLanguage : Functor<CatLanguage> 
    {
        public CatLanguage() { }
        public abstract Functor<CatLanguage, A> Lift<A>(A a);
        public abstract Functor<CatLanguage, B> Map<A, B>(Functor<CatLanguage, A> prev, Func<A, B> ab);
    }

    public class LookForCat : CatLanguage, Functor<LookForCat> 
    {
        public override Functor<CatLanguage, A> Lift<A>(A a) {
            throw new NotImplementedException();
        }

        public Functor<LookForCat, B> Map<A, B>(Functor<LookForCat, A> prev, Func<A, B> ab) {
            throw new NotImplementedException();
        }

        public override Functor<CatLanguage, B> Map<A, B>(Functor<CatLanguage, A> prev, Func<A, B> ab) {
            throw new NotImplementedException();
        }

        Functor<LookForCat, A> Functor<LookForCat>.Lift<A>(A a) {
            throw new NotImplementedException();
        }
    }

    public class ConverseWithCat : CatLanguage, Functor<ConverseWithCat> 
    {
        readonly string _cry;
        
        public ConverseWithCat(string cry) {
            _cry = cry;
        }
        
        public override Functor<CatLanguage, A> Lift<A>(A a) {
            throw new NotImplementedException();
        }

        public Functor<ConverseWithCat, B> Map<A, B>(Functor<ConverseWithCat, A> prev, Func<A, B> ab) {
            throw new NotImplementedException();
        }

        public override Functor<CatLanguage, B> Map<A, B>(Functor<CatLanguage, A> prev, Func<A, B> ab) {
            throw new NotImplementedException();
        }

        Functor<ConverseWithCat, A> Functor<ConverseWithCat>.Lift<A>(A a) {
            throw new NotImplementedException();
        }
    }


    public class Cat {
        public string Name;
        public bool IsAwake;
    }
    
    
    public class Piggo {

        static Free<CatLanguage, Cat> LookForCat()
            => new FreeInst<CatLanguage, Cat>();
        
        //or even better, would be an Id above,
        //which can be cast to the free of any functor

        static Free<CatLanguage, string> Converse(string call)
            => new FreeInst<CatLanguage, string>();


        [Fact]
        public async Task TestCats() {
            var p = from cat in LookForCat()
                    where cat.IsAwake
                    let greeting = $"Meeow {cat.Name}!"
                    from reply in Converse(greeting)
                    select (cat, reply);

            var (_, catReply) = await p.Run();
            catReply.ShouldBe("Meeeeeeow!");
        }
    }

    public interface Transformation<M, N> 
        where M : Functor
        where N : Functor {
        Free<N, A> Transform<A>(Free<M, A> inp);
    }

    public abstract class TransformationBase<M, N> : Transformation<M, N>
        where M : Functor
        where N : Functor
    {
        
        public object Visit(object _)
            => throw new Exception("No visit rule in place...");
        
        Free<N, A> Transformation<M, N>.Transform<A>(Free<M, A> tag)
            => ((dynamic)this).Visit((dynamic)tag);
    }

    public class Cat2Async : TransformationBase<CatLanguage, Async> 
    {
        public Free<Async, A> Visit<A>(Free<CatLanguage, A> fa) {
            throw new NotImplementedException();
        }

//        Async<A> Visit<A>(CatLanguage cat) {
//            throw new NotImplementedException();
//        }
    }

    public static class Interpreters {
        public static Task<R> Run<R>(this Free<CatLanguage, R> prog)
            => prog
                .Transform(new Cat2Async())
                .Run();
        
        public static Task<R> Run<R>(this Free<Async, R> prog)
            => default;
    }
    
    
    
    public static class Extensions {

        public static Free<G, A> Transform<F, G, A>(this Free<F, A> fa, Transformation<F, G> transform)
            where F : Functor
            where G : Functor
            => default;
        
        

        public static Free<FB, C> SelectMany<A, B, C, FA, FB>(this Free<FA, A> src, Func<A, Free<FB, B>> afb, Func<A, B, C> abc)
            where FA : Functor
            where FB : Functor, new()
            => Bind(src,
                a => Bind(afb(a),
                      b => LiftFree<FB, C>(Lift<FB, C>(abc(a, b)))));
        

        public static Free<F, B> Select<A, B, F>(this Free<F, A> src, Func<A, B> fn)
            where F : Functor, new()
            => Bind(src, 
                a => LiftFree<F, B>(Lift<F, B>(fn(a))));
        

        public static Free<F, A> Where<A, F>(this Free<F, A> src, Func<A, bool> fn)
            where F : Functor, new()
            => Bind(src, 
                a => LiftFree<F, A>(Lift<F, A>(a))); //TODO use predicate - do functors have zeros? then we need something more than functor...



        static F Lift<F, A>(A val) where F : Functor, new()
            => (F)new F().Lift(val);

        static Free<FA, A> LiftFree<FA, A>(FA fa)
            where FA : Functor
            => default;

        static Free<FB, B> Bind<FA, A, FB, B>(Free<FA, A> src, Func<A, Free<FB, B>> fn)
            where FA : Functor
            where FB : Functor
            => default;
    }
    
}