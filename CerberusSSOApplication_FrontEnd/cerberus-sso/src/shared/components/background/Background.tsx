import GradientASCII from "@/ui/GradientASCII";
import Grainient from "@/ui/Grainient";
import "./Background.css";

export const Background: React.FC = () => {
  return (
    <div className="app-background">
      <Grainient
        className="app-background__grainient"
        colors={["#2f0b04", "#681a0c", "#9a3412", "#f97316", "#fed7aa"]}
        angle={58}
        flow={1.35}
        silk={0.52}
        grainAmount={0.06}
        softness={0.72}
        scale={0.95}
        speed={2.8}
        grainSize={1.1}
        contrast={0.87}
        grainAnimated={false}
        interactive={false}
        clickSwirl={false}
        intro={false}
      />
        {/* <GradientASCII
          colors={["#7b1c28","#ea7849","#dfb483","#ca5f44"]}
          shape="horizon"
          characters=" 01"
          cellSize={13}
          density={0.81}
          clickEffect="none"
          gradientOpacity={0.81}
          depthOfField={0.37}
          flicker={0.27}
          softness={0}
          flow={0.8}
          speed={0}
          scrambleRadius={0.15}
          intro={false}
      /> */}
    </div>
  );
};
