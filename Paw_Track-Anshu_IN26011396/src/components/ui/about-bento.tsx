"use client";
import { Card } from "@/components/ui/card";
import React from "react";
import { Heart, Calendar, ShieldCheck, Sparkles, ArrowRight, PawPrint } from "lucide-react";
import { Animal } from "@/data/schema";

interface AboutBentoProps {
  animals?: Animal[];
  onBookVisit?: (animal: Animal) => void;
  onApply?: (animal: Animal) => void;
}

export function AboutBento({ animals = [], onBookVisit, onApply }: AboutBentoProps) {
  const featuredPet = animals.find((a) => a.status === "Available") || animals[0];
  const secondPet = animals.filter((a) => a.status === "Available")[1] || animals[1];

  return (
    <section className="bg-[#1C0D07] text-[#FFFDF9] py-12 px-4 rounded-3xl mb-12 shadow-2xl border border-[#43281C]/50 relative overflow-hidden">
      {/* Subtle Background Glow */}
      <div className="absolute -top-32 -left-32 w-96 h-96 bg-[#7F5539]/20 rounded-full blur-3xl pointer-events-none" />
      <div className="absolute -bottom-32 -right-32 w-96 h-96 bg-[#B08968]/15 rounded-full blur-3xl pointer-events-none" />

      <div className="max-w-7xl mx-auto relative z-10">
        <div className="text-center mb-12">
          <div className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-[#43281C] text-[#EADBC8] text-xs font-bold uppercase tracking-wider mb-4 border border-[#7F5539]/40">
            <PawPrint className="w-4 h-4 text-[#D4B896]" /> Meet Our Rescued Companions
          </div>
          <h2 className="text-4xl md:text-5xl font-black text-[#FFFDF9] mb-4 font-outfit tracking-tight">
            Loved Animals Looking for Home
          </h2>
          <p className="text-lg text-[#EADBC8]/80 max-w-2xl mx-auto">
            Experience PawTrack's no-hold adoption process — visit in person first, connect naturally, then make your decision.
          </p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
          {/* Main Hero Pet Card (Col-span 2, Row-span 2) */}
          <Card className="md:col-span-2 md:row-span-2 bg-[#2C1810] rounded-2xl p-8 md:p-10 flex flex-col justify-between border border-[#43281C] relative overflow-hidden group shadow-xl transition-all duration-300 hover:border-[#7F5539]">
            <svg
              width="377"
              height="368"
              className="w-105 fill-[#43281C]/30 absolute -bottom-16 group-hover:rotate-180 duration-1000 ease-in-out -right-16 transition-all"
              viewBox="0 0 377 368"
              fill="none"
              xmlns="http://www.w3.org/2000/svg"
            >
              <path d="M179.692 5.79814C182.635 -1.93287 193.572 -1.93285 196.515 5.79816L229.505 92.466C231.206 96.9342 236.103 99.2928 240.657 97.8366L328.986 69.5929C336.865 67.0735 343.684 75.6242 339.474 82.7452L292.284 162.574C289.851 166.69 291.061 171.99 295.038 174.642L372.192 226.091C379.075 230.68 376.641 241.343 368.449 242.491L276.613 255.369C271.878 256.033 268.489 260.283 268.895 265.047L276.776 357.445C277.479 365.688 267.625 370.433 261.619 364.744L194.293 300.973C190.821 297.686 185.386 297.686 181.914 300.973L114.588 364.744C108.582 370.433 98.7281 365.688 99.4311 357.445L107.312 265.047C107.718 260.283 104.329 256.033 99.5941 255.369L7.7582 242.491C-0.433812 241.343 -2.86746 230.68 4.01488 226.091L81.1687 174.642C85.1465 171.99 86.3561 166.69 83.9231 162.574L36.7325 82.7452C32.523 75.6242 39.342 67.0735 47.2212 69.5929L135.55 97.8366C140.104 99.2928 145.001 96.9342 146.702 92.4659L179.692 5.79814Z" />
            </svg>

            {featuredPet && (
              <div className="space-y-6 relative z-10">
                <div className="flex items-center justify-between">
                  <span className="inline-flex items-center gap-1.5 px-4 py-1.5 rounded-full bg-[#7F5539] text-[#FFFDF9] text-xs font-bold uppercase tracking-wider shadow">
                    <Sparkles className="w-3.5 h-3.5" /> Featured Spotlight
                  </span>
                  <span className="px-3 py-1 rounded-full bg-[#10b981]/20 text-[#34d399] border border-[#10b981]/40 text-xs font-bold">
                    {featuredPet.status}
                  </span>
                </div>

                <div className="relative rounded-xl overflow-hidden h-56 border border-[#43281C]">
                  <img
                    src={featuredPet.imageUrl || "https://images.unsplash.com/photo-1514888286974-6c03e2ca1dba?w=800"}
                    alt={featuredPet.name}
                    className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
                  />
                  <div className="absolute inset-0 bg-gradient-to-t from-[#2C1810] via-transparent to-transparent" />
                </div>

                <div>
                  <h3 className="text-3xl md:text-4xl font-black text-[#FFFDF9] tracking-tight leading-tight">
                    {featuredPet.name}
                  </h3>
                  <p className="text-sm text-[#D4B896] font-semibold mt-1">
                    {featuredPet.species} • {featuredPet.breed} • {featuredPet.age} Years Old
                  </p>
                  <p className="text-sm text-[#EADBC8]/80 mt-2 line-clamp-2">
                    {featuredPet.description}
                  </p>
                </div>

                <div className="pt-4 flex gap-3">
                  {onBookVisit && (
                    <button
                      onClick={() => onBookVisit(featuredPet)}
                      className="flex-1 py-3 px-4 rounded-xl bg-[#7F5539] hover:bg-[#B08968] text-[#FFFDF9] font-bold text-sm flex items-center justify-center gap-2 transition-all shadow-md"
                    >
                      <Calendar className="w-4 h-4" /> Book Visit
                    </button>
                  )}
                  {onApply && (
                    <button
                      onClick={() => onApply(featuredPet)}
                      className="flex-1 py-3 px-4 rounded-xl bg-[#FFFDF9] hover:bg-[#EADBC8] text-[#2C1810] font-bold text-sm flex items-center justify-center gap-2 transition-all shadow-md"
                    >
                      <Heart className="w-4 h-4 text-[#7F5539]" /> Apply Now
                    </button>
                  )}
                </div>
              </div>
            )}
          </Card>

          {/* Growth Stat Card */}
          <Card className="bg-[#7F5539] rounded-2xl p-8 text-[#FFFDF9] flex flex-col border-none justify-between shadow-xl relative overflow-hidden">
            <div className="flex items-center justify-between">
              <span className="text-xs font-black uppercase tracking-widest text-[#EADBC8]">
                Adoption Success
              </span>
              <ShieldCheck className="w-5 h-5 text-[#EADBC8]" />
            </div>
            <div className="space-y-2 my-4">
              <span className="text-5xl font-black tracking-tighter text-[#FFFDF9]">98%</span>
              <p className="text-xs text-[#EADBC8] font-medium">Successful Happy Matches</p>
              <div className="h-2 w-full bg-[#2C1810]/40 rounded-full overflow-hidden">
                <div className="h-full w-11/12 bg-[#FFFDF9] rounded-full shadow-[0_0_12px_rgba(255,255,255,0.8)]" />
              </div>
            </div>
            <div className="text-xs text-[#EADBC8]/90 font-mono">
              Medical & Behavior Verified
            </div>
          </Card>

          {/* Secondary Pet Card */}
          <Card className="bg-[#3D2314] rounded-2xl p-6 text-[#FFFDF9] flex flex-col justify-between border border-[#5C3D2E] shadow-xl">
            {secondPet ? (
              <div className="space-y-3">
                <div className="flex items-center justify-between">
                  <span className="text-xs font-bold uppercase tracking-wider text-[#D4B896]">
                    Available Today
                  </span>
                  <span className="text-xs px-2 py-0.5 rounded bg-[#10b981]/20 text-[#34d399]">
                    {secondPet.species}
                  </span>
                </div>
                <div className="h-28 rounded-lg overflow-hidden border border-[#5C3D2E]">
                  <img
                    src={secondPet.imageUrl || "https://images.unsplash.com/photo-1537151608828-ea2b11777ee8?w=600"}
                    alt={secondPet.name}
                    className="w-full h-full object-cover"
                  />
                </div>
                <h4 className="text-lg font-bold text-[#FFFDF9]">{secondPet.name}</h4>
                <p className="text-xs text-[#EADBC8]/80 line-clamp-2">{secondPet.breed}</p>
                {onBookVisit && (
                  <button
                    onClick={() => onBookVisit(secondPet)}
                    className="w-full py-2 rounded-lg bg-[#5C3D2E] hover:bg-[#7F5539] text-[#FFFDF9] text-xs font-bold transition-all mt-2"
                  >
                    Visit {secondPet.name}
                  </button>
                )}
              </div>
            ) : (
              <div className="flex flex-col justify-center gap-2 h-full text-center">
                <PawPrint className="w-8 h-8 text-[#D4B896] mx-auto" />
                <h4 className="text-lg font-bold">100+ Rescues</h4>
                <p className="text-xs text-[#EADBC8]">Across All Shelter Branches</p>
              </div>
            )}
          </Card>

          {/* Bottom Community Banner */}
          <Card className="md:col-span-2 rounded-2xl p-6 border border-[#5C3D2E] flex flex-row items-center justify-between cursor-pointer bg-[#5C3D2E] hover:bg-[#7F5539] transition-all duration-300 overflow-hidden shadow-xl group">
            <div className="space-y-1 relative z-10 text-[#FFFDF9]">
              <h4 className="text-2xl font-black uppercase tracking-tight">
                No Pressure. Visit First.
              </h4>
              <p className="text-sm text-[#EADBC8]">
                Book a visit slot to meet pets in person before deciding to apply.
              </p>
            </div>
            <div className="w-14 h-14 rounded-full flex items-center justify-center text-xl bg-[#FFFDF9] text-[#2C1810] group-hover:scale-110 transition-all duration-300 relative z-10 shadow-lg flex-shrink-0">
              <ArrowRight className="w-6 h-6 text-[#2C1810]" />
            </div>
          </Card>
        </div>
      </div>
    </section>
  );
}

export default AboutBento;
