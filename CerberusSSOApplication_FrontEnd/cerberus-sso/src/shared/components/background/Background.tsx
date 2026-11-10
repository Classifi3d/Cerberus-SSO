import Dither from "@/ui/Dither";

export const Background: React.FC = () => {
  return (
    <div className="fixed inset-0 w-screen h-screen -z-10 overflow-hidden pointer-events-none">
      <Dither></Dither>
    </div>
  );
};
